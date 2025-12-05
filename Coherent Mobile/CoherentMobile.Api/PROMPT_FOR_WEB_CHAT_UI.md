# 💬 Chat Feature - Web Portal UI Implementation

## 🎯 Task Overview
Build a complete real-time chat system UI for the Coherent Mobile web portal. The backend API and SignalR hub are already implemented and running.

---

## 📋 Requirements

### **Features to Implement:**
1. ✅ Chat conversation list (sidebar)
2. ✅ Chat message area (main content)
3. ✅ Message input with send button
4. ✅ Real-time messaging using SignalR
5. ✅ File/image upload and preview
6. ✅ Online/offline status indicators
7. ✅ Unread message badges
8. ✅ Typing indicators ("User is typing...")
9. ✅ Message read receipts (✓✓)
10. ✅ Reply to messages feature
11. ✅ Delete message option
12. ✅ Timestamp display
13. ✅ Message bubbles (sender on right, receiver on left)
14. ✅ Auto-scroll to latest message
15. ✅ Notification sound on new message

---

## 🛠️ Technical Stack

**Use the existing tech stack of the portal:**
- React / Angular / Vue.js (whatever is already used)
- TypeScript (preferred)
- Tailwind CSS / Material UI / Bootstrap (whatever is already used)
- SignalR Client Library: `@microsoft/signalr`
- Axios for HTTP requests (or existing HTTP client)

---

## 🔌 Backend API Details

### **Base URL:**
```
https://localhost:7162
```

### **SignalR Hub URL:**
```
https://localhost:7162/chatHub
```

### **Authentication:**
All API calls require JWT token in header:
```
Authorization: Bearer {token}
```

### **Available Endpoints:**

#### 1. Get Conversations
```http
GET /api/chat/conversations?pageNumber=1&pageSize=20
```
**Response:**
```json
{
  "conversations": [
    {
      "conversationId": 1,
      "conversationType": "OneToOne",
      "lastMessageAt": "2025-12-03T15:30:00Z",
      "lastMessage": "Hello doctor",
      "unreadCount": 3,
      "isMuted": false,
      "otherUserId": 5,
      "otherUserType": "Doctor",
      "otherUserName": "Dr. Ahmed",
      "otherUserAvatar": "https://...",
      "otherUserIsOnline": true
    }
  ],
  "totalCount": 10
}
```

#### 2. Create/Get Conversation
```http
POST /api/chat/conversations
Content-Type: application/json

{
  "otherUserId": 5,
  "otherUserType": "Doctor",
  "initialMessage": "Hello doctor"
}
```

#### 3. Get Messages
```http
GET /api/chat/conversations/{conversationId}/messages?pageNumber=1&pageSize=50
```
**Response:**
```json
{
  "messages": [
    {
      "messageId": 123,
      "conversationId": 1,
      "senderId": 2,
      "senderType": "Patient",
      "senderName": "John Doe",
      "senderAvatar": "https://...",
      "messageType": "Text",
      "content": "Hello doctor",
      "sentAt": "2025-12-03T15:30:00Z",
      "isDelivered": true,
      "isRead": false,
      "replyToMessageId": null
    }
  ]
}
```

#### 4. Send Message
```http
POST /api/chat/messages
Content-Type: application/json

{
  "conversationId": 1,
  "messageType": "Text",
  "content": "Hello",
  "replyToMessageId": null
}
```

#### 5. Upload File
```http
POST /api/chat/upload
Content-Type: multipart/form-data

FormData: file
```
**Response:**
```json
{
  "fileUrl": "/uploads/chat/abc123.jpg",
  "fileName": "image.jpg",
  "fileSize": 102400
}
```

#### 6. Mark as Read
```http
POST /api/chat/conversations/{id}/mark-read
Content-Type: application/json

[123, 124, 125]  // Array of message IDs
```

#### 7. Delete Message
```http
DELETE /api/chat/messages/{messageId}
```

---

## 🔄 SignalR Integration

### **Install Package:**
```bash
npm install @microsoft/signalr
```

### **Connection Setup:**
```typescript
import * as signalR from "@microsoft/signalr";

const connection = new signalR.HubConnectionBuilder()
  .withUrl("https://localhost:7162/chatHub", {
    accessTokenFactory: () => localStorage.getItem("authToken") || ""
  })
  .withAutomaticReconnect()
  .build();

// Start connection
await connection.start();
console.log("Connected to chat hub");
```

### **Join Conversation:**
```typescript
await connection.invoke("JoinConversation", conversationId);
```

### **Send Message (via SignalR):**
```typescript
await connection.invoke("SendMessage", {
  conversationId: 1,
  messageType: "Text",
  content: "Hello",
  replyToMessageId: null
});
```

### **Listen for Messages:**
```typescript
connection.on("ReceiveMessage", (message) => {
  console.log("New message:", message);
  // Add message to state
  setMessages(prev => [...prev, message]);
  // Play notification sound
  playNotificationSound();
});
```

### **Listen for Typing:**
```typescript
connection.on("UserTyping", (conversationId, userId, userName, isTyping) => {
  if (isTyping) {
    showTypingIndicator(userName);
  } else {
    hideTypingIndicator();
  }
});
```

### **Update Typing Status:**
```typescript
// When user starts typing
await connection.invoke("UpdateTypingStatus", conversationId, true);

// When user stops typing (after 2 seconds of no input)
await connection.invoke("UpdateTypingStatus", conversationId, false);
```

### **Listen for Online/Offline:**
```typescript
connection.on("UserOnline", (userId, userType) => {
  updateUserStatus(userId, true);
});

connection.on("UserOffline", (userId, userType, lastSeenAt) => {
  updateUserStatus(userId, false, lastSeenAt);
});
```

### **Listen for Read Receipts:**
```typescript
connection.on("MessageRead", (messageId, userId, userType, readAt) => {
  updateMessageReadStatus(messageId, true);
});
```

---

## 🎨 UI Component Structure

### **Main Chat Layout:**
```
┌─────────────────────────────────────────────┐
│  Header / Navbar                            │
├──────────┬──────────────────────────────────┤
│          │  Chat Header                     │
│ Chat     │  [Dr. Ahmed] [Online] [...]      │
│ List     ├──────────────────────────────────┤
│          │                                  │
│ Conv 1   │  Message Area                    │
│ Conv 2   │  [Messages scroll here]          │
│ Conv 3   │                                  │
│          ├──────────────────────────────────┤
│          │  Message Input                   │
│          │  [Type...] [📎] [😊] [Send]     │
└──────────┴──────────────────────────────────┘
```

### **Components Needed:**

1. **`ChatLayout.tsx`** - Main container
2. **`ChatSidebar.tsx`** - Conversation list
3. **`ConversationItem.tsx`** - Single conversation in list
4. **`ChatWindow.tsx`** - Main chat area
5. **`ChatHeader.tsx`** - Chat header with user info
6. **`MessageList.tsx`** - Scrollable message container
7. **`MessageBubble.tsx`** - Single message bubble
8. **`MessageInput.tsx`** - Input box with send button
9. **`TypingIndicator.tsx`** - "User is typing..." animation
10. **`FileUpload.tsx`** - File/image upload component
11. **`ImagePreview.tsx`** - Image preview modal

---

## 💻 Component Examples

### **1. ChatService.ts (SignalR Service)**
```typescript
import * as signalR from "@microsoft/signalr";

class ChatService {
  private connection: signalR.HubConnection | null = null;
  
  async connect(token: string) {
    this.connection = new signalR.HubConnectionBuilder()
      .withUrl("https://localhost:7162/chatHub", {
        accessTokenFactory: () => token
      })
      .withAutomaticReconnect()
      .build();

    await this.connection.start();
  }

  onReceiveMessage(callback: (message: any) => void) {
    this.connection?.on("ReceiveMessage", callback);
  }

  onUserTyping(callback: (conversationId: number, userId: number, userName: string, isTyping: boolean) => void) {
    this.connection?.on("UserTyping", callback);
  }

  async sendMessage(conversationId: number, content: string) {
    await this.connection?.invoke("SendMessage", {
      conversationId,
      messageType: "Text",
      content
    });
  }

  async joinConversation(conversationId: number) {
    await this.connection?.invoke("JoinConversation", conversationId);
  }

  async updateTypingStatus(conversationId: number, isTyping: boolean) {
    await this.connection?.invoke("UpdateTypingStatus", conversationId, isTyping);
  }
}

export default new ChatService();
```

### **2. MessageBubble Component (React Example)**
```tsx
interface MessageBubbleProps {
  message: {
    messageId: number;
    senderId: number;
    senderName: string;
    content: string;
    messageType: string;
    fileUrl?: string;
    sentAt: string;
    isRead: boolean;
  };
  isOwn: boolean; // true if current user sent it
}

const MessageBubble: React.FC<MessageBubbleProps> = ({ message, isOwn }) => {
  return (
    <div className={`flex ${isOwn ? 'justify-end' : 'justify-start'} mb-4`}>
      <div className={`max-w-xs lg:max-w-md px-4 py-2 rounded-lg ${
        isOwn ? 'bg-blue-500 text-white' : 'bg-gray-200 text-gray-800'
      }`}>
        {!isOwn && (
          <div className="text-xs font-semibold mb-1">{message.senderName}</div>
        )}
        
        {message.messageType === 'Text' ? (
          <p>{message.content}</p>
        ) : message.messageType === 'Image' ? (
          <img src={message.fileUrl} alt="attachment" className="rounded" />
        ) : (
          <a href={message.fileUrl} className="underline">
            📎 {message.content || 'Download file'}
          </a>
        )}
        
        <div className="text-xs opacity-75 mt-1 flex items-center justify-end">
          <span>{new Date(message.sentAt).toLocaleTimeString()}</span>
          {isOwn && (
            <span className="ml-1">
              {message.isRead ? '✓✓' : '✓'}
            </span>
          )}
        </div>
      </div>
    </div>
  );
};
```

### **3. MessageInput Component (React Example)**
```tsx
const MessageInput: React.FC<{ conversationId: number }> = ({ conversationId }) => {
  const [message, setMessage] = useState('');
  const [isTyping, setIsTyping] = useState(false);
  const typingTimeout = useRef<NodeJS.Timeout>();

  const handleTyping = (value: string) => {
    setMessage(value);
    
    if (!isTyping) {
      setIsTyping(true);
      chatService.updateTypingStatus(conversationId, true);
    }

    // Clear previous timeout
    clearTimeout(typingTimeout.current);
    
    // Set new timeout to stop typing indicator
    typingTimeout.current = setTimeout(() => {
      setIsTyping(false);
      chatService.updateTypingStatus(conversationId, false);
    }, 2000);
  };

  const handleSend = async () => {
    if (!message.trim()) return;
    
    await chatService.sendMessage(conversationId, message);
    setMessage('');
    
    if (isTyping) {
      setIsTyping(false);
      chatService.updateTypingStatus(conversationId, false);
    }
  };

  return (
    <div className="border-t p-4 flex gap-2">
      <input
        type="text"
        value={message}
        onChange={(e) => handleTyping(e.target.value)}
        onKeyPress={(e) => e.key === 'Enter' && handleSend()}
        placeholder="Type a message..."
        className="flex-1 px-4 py-2 border rounded-lg"
      />
      <button
        onClick={handleSend}
        className="px-6 py-2 bg-blue-500 text-white rounded-lg hover:bg-blue-600"
      >
        Send
      </button>
    </div>
  );
};
```

### **4. File Upload**
```tsx
const FileUpload: React.FC<{ onFileUploaded: (fileData: any) => void }> = ({ onFileUploaded }) => {
  const handleFileSelect = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    const formData = new FormData();
    formData.append('file', file);

    const response = await axios.post('/api/chat/upload', formData, {
      headers: {
        'Authorization': `Bearer ${getToken()}`,
        'Content-Type': 'multipart/form-data'
      }
    });

    onFileUploaded(response.data);
  };

  return (
    <label className="cursor-pointer">
      <input type="file" className="hidden" onChange={handleFileSelect} accept="image/*,.pdf,.doc,.docx" />
      <span className="text-2xl">📎</span>
    </label>
  );
};
```

---

## 🎯 Key Features Implementation

### **1. Auto-scroll to Latest Message:**
```typescript
const messagesEndRef = useRef<HTMLDivElement>(null);

useEffect(() => {
  messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
}, [messages]);

// In JSX
<div ref={messagesEndRef} />
```

### **2. Notification Sound:**
```typescript
const playNotificationSound = () => {
  const audio = new Audio('/sounds/notification.mp3');
  audio.play();
};
```

### **3. Unread Badge:**
```tsx
{conversation.unreadCount > 0 && (
  <span className="bg-red-500 text-white rounded-full px-2 py-1 text-xs">
    {conversation.unreadCount}
  </span>
)}
```

### **4. Online Status Indicator:**
```tsx
<div className="flex items-center">
  <div className={`w-3 h-3 rounded-full mr-2 ${
    user.isOnline ? 'bg-green-500' : 'bg-gray-400'
  }`} />
  <span>{user.isOnline ? 'Online' : 'Offline'}</span>
</div>
```

### **5. Typing Indicator:**
```tsx
{isTyping && (
  <div className="text-sm text-gray-500 italic">
    {typingUserName} is typing...
  </div>
)}
```

---

## 📱 Responsive Design

### **Desktop (>768px):**
- Sidebar: 320px width
- Chat area: Remaining width
- 2-column layout

### **Mobile (<768px):**
- Stack layout
- Show conversation list OR chat window
- Back button to return to list
- Full-width messages

---

## 🎨 Design Guidelines

### **Colors:**
- Primary: Blue (#3B82F6)
- Success: Green (#10B981)
- Danger: Red (#EF4444)
- Gray shades for neutral elements

### **Message Bubbles:**
- Own messages: Right-aligned, blue background
- Other messages: Left-aligned, gray background
- Max width: 70% of container
- Border radius: 12px
- Padding: 12px 16px

### **Typography:**
- Message text: 14px
- Timestamps: 11px, opacity 0.7
- User names: 12px, bold

---

## 🧪 Testing Checklist

- [ ] Connect to SignalR hub
- [ ] Load conversation list
- [ ] Click conversation and load messages
- [ ] Send text message
- [ ] Upload and send image
- [ ] Receive real-time messages
- [ ] Show typing indicator
- [ ] Display online/offline status
- [ ] Mark messages as read
- [ ] Show unread count badge
- [ ] Auto-scroll to new messages
- [ ] Play notification sound
- [ ] Reply to message
- [ ] Delete own message
- [ ] Test on mobile responsive
- [ ] Test reconnection after disconnect

---

## 🚀 Implementation Steps

### **Step 1: Setup**
1. Install SignalR: `npm install @microsoft/signalr`
2. Create `services/chatService.ts`
3. Create chat components folder structure

### **Step 2: SignalR Connection**
1. Initialize connection with JWT token
2. Handle connect/disconnect events
3. Set up message listeners

### **Step 3: Conversation List**
1. Fetch conversations from API
2. Display in sidebar
3. Show unread badges
4. Handle click to open chat

### **Step 4: Chat Window**
1. Fetch messages for selected conversation
2. Display messages in bubbles
3. Implement auto-scroll
4. Show typing indicator

### **Step 5: Message Input**
1. Create input component
2. Implement typing detection
3. Send message via SignalR
4. Handle Enter key

### **Step 6: File Upload**
1. Add file input
2. Upload to API
3. Send file message
4. Display file preview

### **Step 7: Real-time Features**
1. Listen for new messages
2. Update UI instantly
3. Play notification sound
4. Update read status

### **Step 8: Polish**
1. Add loading states
2. Error handling
3. Empty states
4. Animations
5. Mobile responsive

---

## 📦 Dependencies

```json
{
  "dependencies": {
    "@microsoft/signalr": "^7.0.0",
    "axios": "^1.6.0",
    "react": "^18.2.0",
    "react-dom": "^18.2.0"
  }
}
```

---

## 🔐 Authentication

Get JWT token from localStorage after login:
```typescript
const token = localStorage.getItem('authToken');
```

Include in all API calls:
```typescript
headers: {
  'Authorization': `Bearer ${token}`
}
```

---

## 🎯 Expected Output

A fully functional real-time chat system with:
- ✅ Beautiful, modern UI
- ✅ Real-time messaging (no refresh needed)
- ✅ File sharing
- ✅ Typing indicators
- ✅ Online status
- ✅ Read receipts
- ✅ Mobile responsive
- ✅ Notification sounds
- ✅ Smooth animations

---

## 📞 Support

**Backend API:** https://localhost:7162
**SignalR Hub:** https://localhost:7162/chatHub
**API Documentation:** See TEST_CHAT.http for examples

---

**Start building and create an amazing chat experience! 🚀💬**
