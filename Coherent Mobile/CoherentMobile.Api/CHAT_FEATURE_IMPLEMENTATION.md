# 💬 **Chat Feature Implementation Guide**

## 📋 **Overview**

Real-time chat system for **Coherent Mobile** - supporting both **Web Portal** and **Mobile App**.

---

## 🎯 **Features**

### **✅ Core Features:**
1. ✅ One-to-one messaging (Patient ↔️ Doctor)
2. ✅ Support chat (Patient ↔️ Staff)
3. ✅ Real-time messaging (SignalR)
4. ✅ Message delivery & read receipts
5. ✅ Online/Offline status
6. ✅ Chat history & pagination
7. ✅ File sharing (Images, Documents)
8. ✅ Message replies
9. ✅ Unread message counter
10. ✅ Mute conversations

### **📱 Supported Platforms:**
- Web Portal (React/Angular)
- iOS Mobile App
- Android Mobile App

---

## 🗄️ **Database Schema**

### **Tables Created:**

#### **1. MConversations**
Stores chat conversations
```sql
- ConversationId (PK)
- ConversationType (OneToOne/Group/Support)
- Title (for group chats)
- CreatedBy
- LastMessageAt
- LastMessage
- IsActive
- IsArchived
```

#### **2. MConversationParticipants**
Links users to conversations
```sql
- ParticipantId (PK)
- ConversationId (FK)
- UserId
- UserType (Patient/Doctor/Staff)
- JoinedAt
- IsActive
- IsMuted
- LastReadMessageId
- UnreadCount
```

#### **3. MChatMessages**
Stores all messages
```sql
- MessageId (PK)
- ConversationId (FK)
- SenderId
- SenderType
- MessageType (Text/Image/File/Voice/Video)
- Content
- FileUrl, FileName, FileSize
- SentAt
- IsDelivered, IsRead
- IsDeleted
- ReplyToMessageId
```

#### **4. MMessageReceipts**
Tracks message delivery/read status
```sql
- ReceiptId (PK)
- MessageId (FK)
- UserId
- UserType
- DeliveredAt
- ReadAt
```

#### **5. MUserStatus**
Tracks online/offline status
```sql
- StatusId (PK)
- UserId
- UserType
- IsOnline
- LastSeenAt
- ConnectionId (SignalR)
- DeviceType
```

---

## 📡 **API Endpoints**

### **Chat Controller** (`/api/chat`)

#### **1. Get User Conversations**
```http
GET /api/chat/conversations?pageNumber=1&pageSize=20
Authorization: Bearer {token}
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
  "totalCount": 10,
  "pageNumber": 1,
  "pageSize": 20
}
```

---

#### **2. Create or Get Conversation**
```http
POST /api/chat/conversations
Authorization: Bearer {token}
Content-Type: application/json

{
  "otherUserId": 5,
  "otherUserType": "Doctor",
  "initialMessage": "Hello doctor"
}
```

**Response:**
```json
{
  "conversationId": 1,
  "message": "Conversation created successfully"
}
```

---

#### **3. Get Conversation Messages**
```http
GET /api/chat/conversations/1/messages?pageNumber=1&pageSize=50
Authorization: Bearer {token}
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
  ],
  "totalCount": 50,
  "pageNumber": 1
}
```

---

#### **4. Send Message**
```http
POST /api/chat/messages
Authorization: Bearer {token}
Content-Type: application/json

{
  "conversationId": 1,
  "messageType": "Text",
  "content": "Hello doctor",
  "replyToMessageId": null
}
```

**Response:**
```json
{
  "messageId": 124,
  "sentAt": "2025-12-03T15:31:00Z",
  "success": true
}
```

---

#### **5. Mark Messages as Read**
```http
POST /api/chat/conversations/1/mark-read
Authorization: Bearer {token}
Content-Type: application/json

{
  "messageIds": [123, 124, 125]
}
```

---

#### **6. Upload File**
```http
POST /api/chat/upload
Authorization: Bearer {token}
Content-Type: multipart/form-data

file: [binary data]
```

**Response:**
```json
{
  "fileUrl": "https://storage.../file.jpg",
  "fileName": "file.jpg",
  "fileSize": 102400
}
```

---

## 🔄 **SignalR Hub** (`/chatHub`)

### **Hub Methods:**

#### **Client → Server:**

**1. JoinConversation**
```javascript
connection.invoke("JoinConversation", conversationId);
```

**2. LeaveConversation**
```javascript
connection.invoke("LeaveConversation", conversationId);
```

**3. SendMessage**
```javascript
connection.invoke("SendMessage", {
  conversationId: 1,
  messageType: "Text",
  content: "Hello",
  replyToMessageId: null
});
```

**4. UpdateTypingStatus**
```javascript
connection.invoke("UpdateTypingStatus", conversationId, true);
```

**5. MarkAsRead**
```javascript
connection.invoke("MarkAsRead", conversationId, messageIds);
```

---

#### **Server → Client:**

**1. ReceiveMessage**
```javascript
connection.on("ReceiveMessage", (message) => {
  console.log("New message:", message);
  // Update UI
});
```

**2. MessageDelivered**
```javascript
connection.on("MessageDelivered", (messageId) => {
  // Update message status
});
```

**3. MessageRead**
```javascript
connection.on("MessageRead", (messageId, userId) => {
  // Update message read status
});
```

**4. UserTyping**
```javascript
connection.on("UserTyping", (userId, userName, isTyping) => {
  // Show/hide "User is typing..." indicator
});
```

**5. UserOnline**
```javascript
connection.on("UserOnline", (userId, userType) => {
  // Update online status
});
```

**6. UserOffline**
```javascript
connection.on("UserOffline", (userId, userType, lastSeenAt) => {
  // Update offline status
});
```

---

## 💻 **Frontend Implementation**

### **Web Portal (React Example):**

```typescript
import * as signalR from "@microsoft/signalr";

// 1. Create SignalR Connection
const connection = new signalR.HubConnectionBuilder()
  .withUrl("https://localhost:7162/chatHub", {
    accessTokenFactory: () => getAuthToken()
  })
  .withAutomaticReconnect()
  .build();

// 2. Start Connection
await connection.start();

// 3. Join Conversation
await connection.invoke("JoinConversation", conversationId);

// 4. Listen for Messages
connection.on("ReceiveMessage", (message) => {
  setMessages(prev => [...prev, message]);
});

// 5. Send Message
const sendMessage = async (content) => {
  await connection.invoke("SendMessage", {
    conversationId,
    messageType: "Text",
    content
  });
};

// 6. Update Typing Status
const setTyping = (isTyping) => {
  connection.invoke("UpdateTypingStatus", conversationId, isTyping);
};
```

---

### **Mobile App (React Native Example):**

```typescript
import { HubConnectionBuilder } from '@microsoft/signalr';

// Same as web, but add:
// - Push notification handling
// - Image picker for attachments
// - Local message storage

const ChatScreen = () => {
  const [connection, setConnection] = useState(null);
  const [messages, setMessages] = useState([]);

  useEffect(() => {
    const newConnection = new HubConnectionBuilder()
      .withUrl("https://api.yourserver.com/chatHub", {
        accessTokenFactory: () => getToken()
      })
      .withAutomaticReconnect()
      .build();

    newConnection.start()
      .then(() => {
        console.log('Connected to SignalR');
        newConnection.invoke("JoinConversation", conversationId);
      });

    newConnection.on("ReceiveMessage", (message) => {
      setMessages(prev => [...prev, message]);
      // Play notification sound
      // Show push notification if app is in background
    });

    setConnection(newConnection);

    return () => {
      newConnection.stop();
    };
  }, []);

  return (
    <View>
      <FlatList data={messages} renderItem={MessageBubble} />
      <MessageInput onSend={sendMessage} />
    </View>
  );
};
```

---

## 📦 **Backend Files Created**

### **✅ Already Created:**

1. **Database:**
   - `07_CreateChatTables.sql` - Database schema with tables and stored procedures

2. **DTOs:**
   - `ConversationDto.cs` - Conversation model
   - `ChatMessageDto.cs` - Message model
   - `SendMessageRequestDto.cs` - Send message request
   - `CreateConversationRequestDto.cs` - Create conversation request
   - `GetMessagesRequestDto.cs` - Get messages request
   - `GetConversationsResponseDto.cs` - Get conversations response

### **🔨 To Be Created:**

3. **Domain Models:**
   - `Conversation.cs`
   - `ChatMessage.cs`
   - `ConversationParticipant.cs`

4. **Repositories:**
   - `IChatRepository.cs` (Interface)
   - `ChatRepository.cs` (Implementation)

5. **Services:**
   - `IChatService.cs` (Interface)
   - `ChatService.cs` (Implementation)

6. **SignalR:**
   - `ChatHub.cs` - SignalR hub for real-time messaging
   - `IChat Client.cs` - Client interface

7. **Controllers:**
   - `ChatController.cs` - REST API endpoints

8. **Validators:**
   - `SendMessageRequestValidator.cs`
   - `CreateConversationRequestValidator.cs`

---

## ⚙️ **Configuration**

### **appsettings.json:**
```json
{
  "ChatSettings": {
    "MaxFileSize": 10485760,
    "AllowedFileTypes": [".jpg", ".jpeg", ".png", ".pdf", ".doc", ".docx"],
    "MessagePageSize": 50,
    "ConversationPageSize": 20
  },
  "SignalR": {
    "EnableDetailedErrors": true
  }
}
```

### **Program.cs:**
```csharp
// Add SignalR
builder.Services.AddSignalR();

// Map SignalR Hub
app.MapHub<ChatHub>("/chatHub");
```

---

## 🧪 **Testing**

### **Test with REST Client:**

See file: `TEST_CHAT.http`

### **Test with SignalR Client:**

1. Web: Use browser console with SignalR client library
2. Mobile: Use SignalR mobile SDK
3. Postman: Use WebSocket feature

---

## 📱 **Mobile App Requirements**

### **Packages Needed:**

**React Native:**
```bash
npm install @microsoft/signalr
npm install react-native-image-picker
npm install @notifee/react-native  # For notifications
```

**Flutter:**
```yaml
dependencies:
  signalr_netcore: ^1.3.3
  image_picker: ^0.8.7+4
  flutter_local_notifications: ^14.0.0
```

---

## 🔐 **Security**

1. ✅ JWT Authentication required
2. ✅ User can only access their own conversations
3. ✅ File upload validation (type, size)
4. ✅ Message content sanitization
5. ✅ Rate limiting on SignalR connections

---

## 🚀 **Next Steps**

### **Backend (Mera kaam):**
- [ ] Create domain models
- [ ] Create repository layer
- [ ] Create service layer
- [ ] Create SignalR hub
- [ ] Create API controller
- [ ] Add validators
- [ ] Create test file
- [ ] Add file upload support

### **Frontend (Tumhara kaam):**
- [ ] Create chat UI components
- [ ] Integrate SignalR client
- [ ] Handle real-time messages
- [ ] Add file upload
- [ ] Add push notifications
- [ ] Test on mobile devices

---

**Status:** 🟡 In Progress  
**Last Updated:** December 3, 2025
