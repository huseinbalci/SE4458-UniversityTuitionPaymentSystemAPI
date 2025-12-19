import React, { useState, useRef, useEffect } from "react";
import { v4 as uuidv4 } from "uuid"; // for sessionId

export default function ChatBox() {
    const [input, setInput] = useState("");
    const [messages, setMessages] = useState([]);
    const [token, setToken] = useState(null);
    const [studentInfo, setStudentInfo] = useState(null);
    const [showLogin, setShowLogin] = useState(false);
    const [loginRole, setLoginRole] = useState(null);
    const [showUpload, setShowUpload] = useState(false);
    const [uploadFile, setUploadFile] = useState(null);
    const [authStatus, setAuthStatus] = useState("Guest");
    const [sessionId] = useState(uuidv4());
    const messagesEndRef = useRef(null);

    // Azure-ready environment variables
    const CHAT_API_URL = process.env.REACT_APP_CHAT_API_URL || "http://localhost:3001/api/chat";
    const AUTH_API_URL = process.env.REACT_APP_AUTH_API_URL || "http://localhost:5025/api/v1/auth/token";
    const CSV_UPLOAD_URL = process.env.REACT_APP_CSV_UPLOAD_URL || "http://localhost:5025/api/v1/admin/add-tuition-batch";

    const adminIntents = [
        "add tuition", "add student", "add new tuition",
        "add tuition batch", "upload csv", "csv", "batch", "unpaid tuition"
    ];

    const studentIntents = [
        "pay tuition", "pay a tuition", "payment", "my tuition", "my balance"
    ];

    const handleSend = async () => {
        if (!input.trim()) return;

        setMessages(prev => [...prev, { sender: "user", text: input }]);
        const inputLower = input.toLowerCase();
        setInput("");

        const requiresAdmin = adminIntents.some(i => inputLower.includes(i));
        const requiresStudent = studentIntents.some(i => inputLower.includes(i));

        if (requiresAdmin && !token) {
            setLoginRole("Admin");
            setShowLogin(true);
            setMessages(prev => [...prev, { sender: "bot", text: "Admin login required." }]);
            return;
        }

        if (requiresStudent && !token) {
            setLoginRole("Student");
            setShowLogin(true);
            setMessages(prev => [...prev, { sender: "bot", text: "Student login required." }]);
            return;
        }

        if (requiresAdmin && (inputLower.includes("csv") || inputLower.includes("batch"))) {
            setShowUpload(true);
            setMessages(prev => [...prev, { sender: "bot", text: "Upload CSV file to continue." }]);
            return;
        }

        const payload = { prompt: input, sessionId };
        if (token && loginRole === "Student" && studentInfo) {
            payload.studentNo = studentInfo.studentNo;
            payload.fullName = studentInfo.fullName;
        }

        try {
            const response = await fetch(CHAT_API_URL, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    ...(token && { Authorization: `Bearer ${token}` })
                },
                body: JSON.stringify(payload)
            });

            if (!response.ok) throw new Error("Request failed");

            const data = await response.json();
            setMessages(prev => [...prev, { sender: "bot", text: data.output }]);
        } catch {
            setMessages(prev => [...prev, { sender: "bot", text: "An error occurred." }]);
        }
    };

    const handleLogin = async (username, password) => {
        try {
            const response = await fetch(AUTH_API_URL, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ Username: username, Password: password })
            });

            const data = await response.json();
            if (!data.token) throw new Error("Invalid credentials");

            setToken(data.token);

            if (data.studentNo && data.fullName) {
                setStudentInfo({ studentNo: data.studentNo, fullName: data.fullName });
                setAuthStatus(`${data.fullName}-${data.studentNo}`);
            } else {
                setAuthStatus("Admin");
            }

            setShowLogin(false);
            setMessages(prev => [...prev, { sender: "bot", text: "Login successful." }]);
        } catch {
            alert("Login failed");
        }
    };

    const handleCsvUpload = async () => {
        if (!uploadFile) return;
        const formData = new FormData();
        formData.append("file", uploadFile);

        try {
            const response = await fetch(CSV_UPLOAD_URL, {
                method: "POST",
                headers: { Authorization: `Bearer ${token}` },
                body: formData
            });

            if (!response.ok) throw new Error();
            setMessages(prev => [...prev, { sender: "bot", text: "All tuitions added successfully." }]);
            setShowUpload(false);
            setUploadFile(null);
        } catch {
            setMessages(prev => [...prev, { sender: "bot", text: "CSV upload failed." }]);
        }
    };

    const handleLogout = () => {
        setToken(null);
        setStudentInfo(null);
        setAuthStatus("Guest");
        setLoginRole(null);
        setShowLogin(false);
        setShowUpload(false);
        setMessages(prev => [...prev, { sender: "bot", text: "Signed out." }]);
    };

    useEffect(() => {
        messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
    }, [messages]);

    return (
        <div className="flex flex-col max-w-xl mx-auto h-[600px] border rounded-2xl shadow-lg bg-gray-100">
            <div className="flex justify-between items-center p-4 bg-green-600 text-white rounded-t-2xl font-semibold">
                <span>Yaşar Chat Assistant</span>
                <div className="flex gap-3 items-center text-sm">
                    <span>{authStatus}</span>
                    {token && (
                        <button
                            onClick={handleLogout}
                            className="bg-white text-green-600 px-3 py-1 rounded-full"
                        >
                            Sign out
                        </button>
                    )}
                </div>
            </div>

            <div className="flex-1 overflow-y-auto p-4 space-y-3">
                {messages.map((m, i) => (
                    <div
                        key={i}
                        className={`max-w-[75%] px-4 py-2 rounded-2xl ${m.sender === "user" ? "ml-auto bg-green-500 text-white" : "mr-auto bg-white"}`}
                    >
                        {m.text}
                    </div>
                ))}
                <div ref={messagesEndRef} />
            </div>

            {showLogin ? (
                <div className="p-4 bg-white border-t">
                    <form
                        onSubmit={e => {
                            e.preventDefault();
                            handleLogin(e.target.u.value, e.target.p.value);
                        }}
                    >
                        <input name="u" className="mb-2 p-2 border w-full" required />
                        <input name="p" type="password" className="mb-2 p-2 border w-full" required />
                        <button className="w-full bg-green-600 text-white py-2 rounded-full">Login</button>
                    </form>
                </div>
            ) : showUpload ? (
                <div className="p-4 bg-white border-t">
                    <input type="file" accept=".csv" onChange={e => setUploadFile(e.target.files[0])} />
                    <button onClick={handleCsvUpload} className="w-full mt-2 bg-green-600 text-white py-2 rounded-full">Upload CSV</button>
                </div>
            ) : (
                <div className="p-3 bg-white border-t flex gap-2">
                    <input
                        value={input}
                        onChange={e => setInput(e.target.value)}
                        onKeyDown={e => e.key === "Enter" && handleSend()}
                        className="flex-1 px-4 py-2 border rounded-full"
                    />
                    <button onClick={handleSend} className="bg-green-600 text-white px-4 py-2 rounded-full">Send</button>
                </div>
            )}
        </div>
    );
}
