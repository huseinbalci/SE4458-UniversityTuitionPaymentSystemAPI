const express = require("express");
const cors = require("cors");
require("dotenv").config();
const Groq = require("groq-sdk");

const fetch = (...args) =>
    import("node-fetch").then(({ default: fetch }) => fetch(...args));

const app = express();
app.use(cors());
app.use(express.json());

const GROQ_API_KEY = System.Environment.GetEnvironmentVariable("GROQ_API_KEY");

if (!GROQ_API_KEY) {
    console.error("GROQ_API_KEY not set!");
    process.exit(1);
} else {
    console.log("GROQ_API_KEY loaded successfully"); // You will see this in log-stream
}

console.log("AI server running...");

const groq = new Groq({ apiKey: GROQ_API_KEY });

const sessionMemory = {};

const tools = [
    {
        type: "function",
        function: {
            name: "query_tuition",
            description: "Get tuition information for a student and optionally a term",
            parameters: {
                type: "object",
                properties: {
                    studentNo: { type: "string" },
                    term: { type: "string" }
                },
                required: ["studentNo"]
            }
        }
    },
    {
        type: "function",
        function: {
            name: "pay_tuition",
            description: "Pay tuition for a student for a specific term",
            parameters: {
                type: "object",
                properties: {
                    studentNo: { type: "string" },
                    term: { type: "string" },
                    amount: { type: "number" }
                },
                required: ["studentNo", "term", "amount"]
            }
        }
    },
    {
        type: "function",
        function: {
            name: "add_student",
            description: "Add a new student (admin only)",
            parameters: {
                type: "object",
                properties: {
                    studentNo: { type: "string" },
                    fullName: { type: "string" }
                },
                required: ["studentNo", "fullName"]
            }
        }
    },
    {
        type: "function",
        function: {
            name: "add_tuition",
            description: "Add tuition for a student and term (admin only)",
            parameters: {
                type: "object",
                properties: {
                    studentNo: { type: "string" },
                    term: { type: "string" },
                    totalAmount: { type: "number" }
                },
                required: ["studentNo", "term", "totalAmount"]
            }
        }
    },
    {
        type: "function",
        function: {
            name: "get_unpaid_tuition",
            description: "Get unpaid tuition list with pagination (admin only)",
            parameters: {
                type: "object",
                properties: {
                    page: { type: "number" },
                    pageSize: { type: "number" }
                }
            }
        }
    }
];

async function parseJsonSafe(res) {
    const text = await res.text();
    try {
        return JSON.parse(text);
    } catch {
        return { message: text };
    }
}

app.post("/api/chat", async (req, res) => {
    const { prompt, sessionId, studentNo, fullName } = req.body;
    if (!prompt) return res.status(400).json({ error: "Prompt is required" });

    if (!sessionMemory[sessionId]) {
        sessionMemory[sessionId] = { lastStudentNo: null, lastFullName: null, lastTerm: null };
    }

    if (studentNo) sessionMemory[sessionId].lastStudentNo = studentNo;
    if (fullName) sessionMemory[sessionId].lastFullName = fullName;

    try {
        const systemMessage = sessionMemory[sessionId].lastStudentNo
            ? `You are a university tuition assistant.
               The logged-in user is: ${sessionMemory[sessionId].lastFullName} (${sessionMemory[sessionId].lastStudentNo}).
               Respond clearly and concisely to tuition queries or admin actions.
               Note: "Add tuition" means the same as "add new tuition".`
            : `You are a university tuition assistant.
               Greet the user.
               If user asks, tell a joke or make a casual conversation.
               Respond clearly and concisely.
               Currently no user is logged in.
               Note: "Add tuition" means the same as "add new tuition".`;

        const completion = await groq.chat.completions.create({
            model: "llama-3.3-70b-versatile",
            messages: [
                { role: "system", content: systemMessage },
                { role: "user", content: prompt }
            ],
            tools,
            tool_choice: "auto"
        });

        const message = completion.choices[0].message;

        if (!message.tool_calls || message.tool_calls.length === 0) {
            return res.json({ output: message.content || "" });
        }

        const toolCall = message.tool_calls[0];
        const args = JSON.parse(toolCall.function.arguments || "{}");

        if (!args.studentNo && sessionMemory[sessionId].lastStudentNo) {
            args.studentNo = sessionMemory[sessionId].lastStudentNo;
        }
        if (!args.fullName && sessionMemory[sessionId].lastFullName) {
            args.fullName = sessionMemory[sessionId].lastFullName;
        }
        if (!args.term && sessionMemory[sessionId].lastTerm) {
            args.term = sessionMemory[sessionId].lastTerm;
        }

        if (args.studentNo) sessionMemory[sessionId].lastStudentNo = args.studentNo;
        if (args.fullName) sessionMemory[sessionId].lastFullName = args.fullName;
        if (args.term) sessionMemory[sessionId].lastTerm = args.term;

        const authHeader = req.headers.authorization;
        let toolResult;

        switch (toolCall.function.name) {
            case "query_tuition": {
                const url = `http://localhost:5025/api/v1/mobile/query-tuition?studentNo=${args.studentNo}${args.term ? `&term=${args.term}` : ""}`;
                const apiRes = await fetch(url, { headers: authHeader ? { Authorization: authHeader } : {} });
                toolResult = await parseJsonSafe(apiRes);
                break;
            }
            case "pay_tuition": {
                const url = `http://localhost:5025/api/v1/bank/pay-tuition?studentNo=${args.studentNo}&term=${args.term}&amount=${args.amount}`;
                const apiRes = await fetch(url, { method: "POST", headers: authHeader ? { Authorization: authHeader } : {} });
                toolResult = await parseJsonSafe(apiRes);
                break;
            }
            case "add_student": {
                const url = `http://localhost:5025/api/v1/admin/add-student?studentNo=${args.studentNo}&fullName=${encodeURIComponent(args.fullName)}`;
                const apiRes = await fetch(url, { method: "POST", headers: { Authorization: authHeader } });
                toolResult = await parseJsonSafe(apiRes);
                break;
            }
            case "add_tuition": {
                const url = `http://localhost:5025/api/v1/admin/add-tuition?studentNo=${args.studentNo}&term=${args.term}&totalAmount=${args.totalAmount}`;
                const apiRes = await fetch(url, { method: "POST", headers: { Authorization: authHeader } });
                toolResult = await parseJsonSafe(apiRes);
                break;
            }
            case "get_unpaid_tuition": {
                const page = args.page || 1;
                const pageSize = args.pageSize || 10;
                const url = `http://localhost:5025/api/v1/admin/unpaid?page=${page}&pageSize=${pageSize}`;
                const apiRes = await fetch(url, { headers: { Authorization: authHeader } });
                toolResult = await parseJsonSafe(apiRes);
                break;
            }
            default:
                throw new Error("Unknown tool");
        }

        const finalCompletion = await groq.chat.completions.create({
            model: "llama-3.3-70b-versatile",
            messages: [
                { role: "system", content: systemMessage },
                { role: "user", content: prompt },
                message,
                { role: "tool", tool_call_id: toolCall.id, content: JSON.stringify(toolResult) }
            ]
        });

        res.json({ output: finalCompletion.choices[0].message.content });
    } catch (err) {
        console.error(err);
        res.status(500).json({ error: "Internal server error" });
    }
});

const PORT = process.env.PORT || 3001;
app.listen(PORT, () => {
    console.log(`AI server running on port ${PORT}`);
});


