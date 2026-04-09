# Email Service Setup and Project Invitation Guide

## Tổng quan

Hệ thống IssueTracker hỗ trợ đầy đủ việc mời người dùng vào dự án qua email, bao gồm cả trường hợp **người nhận chưa có tài khoản**. 

### Flow hoạt động:

**Trường hợp 1: Người nhận ĐÃ CÓ tài khoản**
1. Người gửi invite user qua email
2. Hệ thống tạo invitation record (status: Pending)
3. Gửi email thông báo với link đăng nhập
4. Người nhận đăng nhập → xem danh sách invitations → accept/reject
5. Khi accept: tạo UserProject record, update invitation status = Accepted

**Trường hợp 2: Người nhận CHƯA CÓ tài khoản**
1. Người gửi invite user qua email
2. Hệ thống tạo invitation record (status: Pending)
3. Gửi email thông báo với hướng dẫn đăng ký
4. Người nhận đăng ký tài khoản (PHẢI dùng đúng email được mời)
5. Sau khi đăng nhập → xem danh sách invitations → accept/reject
6. Khi accept: tạo UserProject record, update invitation status = Accepted

**Điểm quan trọng**: Invitation được lưu theo email, không phụ thuộc vào user ID, nên người nhận có thể đăng ký sau khi được mời.

## Cấu hình Email Service

### 1. Cấu hình SMTP Settings

Thêm cấu hình sau vào file `appsettings.json`:

```json
{
  "EmailSettings": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": "587",
    "SmtpUsername": "your-email@gmail.com",
    "SmtpPassword": "your-app-password",
    "FromEmail": "your-email@gmail.com",
    "FromName": "IssueTracker System"
  },
  "AppSettings": {
    "FrontendUrl": "http://localhost:3000"
  }
}
```

### 2. Sử dụng Gmail SMTP

Nếu sử dụng Gmail:

1. Bật xác thực 2 yếu tố (2FA) cho tài khoản Gmail
2. Tạo App Password tại: https://myaccount.google.com/apppasswords
3. Sử dụng App Password thay vì mật khẩu Gmail thông thường

**Lưu ý**: KHÔNG commit file appsettings với thông tin email thật vào Git. Chỉ commit file Example.

### 3. Development Mode

Trong môi trường development, nếu không cấu hình SMTP credentials (`SmtpUsername` và `SmtpPassword` để trống), email sẽ không được gửi thực sự. Thay vào đó, nội dung email sẽ được ghi vào log để bạn có thể kiểm tra.

## API Endpoints

### 1. Invite User to Project

**POST** `/api/v1/projects/{projectId}/invite`

**Headers:**
```
Authorization: Bearer {token}
```

**Request Body:**
```json
{
  "recipientEmail": "user@example.com"
}
```

**Response:**
```json
{
  "id": "guid",
  "projectId": "guid",
  "senderId": "guid",
  "senderEmail": "sender@example.com",
  "senderName": "Sender Full Name",
  "recipientEmail": "user@example.com",
  "statusOfInvitation": 0
}
```

### 2. Get Pending Invitations

**GET** `/api/v1/projects/invitations`

Lấy danh sách tất cả invitations pending cho email của user hiện tại.

**Headers:**
```
Authorization: Bearer {token}
```

**Response:**
```json
[
  {
    "id": "guid",
    "projectId": "guid",
    "senderId": "guid",
    "senderEmail": "sender@example.com",
    "senderName": "Sender Name",
    "recipientEmail": "current-user@example.com",
    "statusOfInvitation": 0
  }
]
```

### 3. Accept Invitation

**POST** `/api/v1/projects/invitations/{invitationId}/accept`

Chấp nhận lời mời và thêm user vào project với role Developer.

**Headers:**
```
Authorization: Bearer {token}
```

**Response:**
```json
{
  "success": true,
  "message": "Invitation accepted successfully"
}
```

### 4. Reject Invitation

**POST** `/api/v1/projects/invitations/{invitationId}/reject`

Từ chối lời mời.

**Headers:**
```
Authorization: Bearer {token}
```

**Response:**
```json
{
  "success": true,
  "message": "Invitation rejected successfully"
}
```

## Validation Rules

### Invite User (POST /projects/{projectId}/invite)

1. **User phải authenticated**: Người gửi phải đăng nhập
2. **Project phải tồn tại**: Nếu project không tồn tại, API trả về 404
3. **Không được mời duplicate**: Nếu đã có invitation pending cho email này ở project này, API trả về lỗi
4. **Không được mời member hiện tại**: Nếu người được mời đã là member của project, API trả về lỗi

### Accept Invitation (POST /invitations/{invitationId}/accept)

1. **User phải authenticated**: Phải đăng nhập
2. **Invitation phải tồn tại**: Invitation ID phải hợp lệ
3. **Email phải khớp**: Invitation phải dành cho email của user hiện tại
4. **Status phải là Pending**: Không thể accept invitation đã được xử lý
5. **Chưa là member**: User chưa là member of project
6. **Project role phải tồn tại**: Role "Developer" phải có trong database

### Reject Invitation (POST /invitations/{invitationId}/reject)

1. **User phải authenticated**: Phải đăng nhập
2. **Invitation phải tồn tại**: Invitation ID phải hợp lệ
3. **Email phải khớp**: Invitation phải dành cho email của user hiện tại
4. **Status phải là Pending**: Không thể reject invitation đã được xử lý

## Email Template

Email được gửi sẽ bao gồm hướng dẫn cho CẢ HAI trường hợp:

```
Subject: Lời mời tham gia dự án - IssueTracker

Xin chào,

Từ hệ thống IssueTracker: sender@example.com vừa mời bạn tham gia dự án "Project Name".

--- HƯỚNG DẪN ---

● Nếu bạn ĐÃ CÓ TÀI KHOẢN:
  1. Đăng nhập vào hệ thống IssueTracker tại: http://localhost:3000/login
  2. Vào mục "Lời mời" (Invitations) để xem và chấp nhận lời mời

● Nếu bạn CHƯA CÓ TÀI KHOẢN:
  1. Đăng ký tài khoản tại: http://localhost:3000/register
  2. QUAN TRỌNG: Sử dụng đúng email này (recipient@example.com) khi đăng ký
  3. Sau khi đăng ký và đăng nhập, vào mục "Lời mời" để chấp nhận

Lưu ý: Lời mời này chỉ có hiệu lực khi bạn sử dụng đúng email recipient@example.com

---

Nếu bạn không mong muốn nhận lời mời này, bạn có thể bỏ qua email này.

Trân trọng,
IssueTracker System
```

## Kiến trúc

### Files đã tạo/sửa đổi:

**Interfaces & Services:**
1. **IEmailService.cs** - Interface định nghĩa contract cho email service
   - `SendEmailAsync()`: Gửi email chung
   - `SendProjectInvitationEmailAsync()`: Gửi email mời vào project với hướng dẫn chi tiết

2. **EmailService.cs** - Implementation của IEmailService
   - Sử dụng System.Net.Mail.SmtpClient
   - Hỗ trợ SSL/TLS
   - Log email content khi không có SMTP credentials (development)
   - Đọc FrontendUrl từ configuration để tạo links

**Commands:**
3. **InviteUserToProjectCommandHandler.cs** - MediatR handler gửi invitation
   - Validate project existence
   - Validate duplicate invitation
   - Validate existing membership
   - Tạo invitation record (không yêu cầu recipient phải tồn tại)
   - Gửi email
   - Trả về InvitationDto

4. **AcceptInvitationCommandHandler.cs** - MediatR handler chấp nhận invitation
   - Validate invitation ownership (email phải khớp)
   - Validate invitation status (phải pending)
   - Validate không trùng membership
   - Tạo UserProject record với role Developer
   - Update invitation status = Accepted

5. **RejectInvitationCommandHandler.cs** - MediatR handler từ chối invitation
   - Validate invitation ownership (email phải khớp)
   - Validate invitation status (phải pending)
   - Update invitation status = Rejected

**Queries:**
6. **GetPendingInvitationsQuery.cs** - Lấy danh sách invitations pending
   - Filter theo email của current user
   - Chỉ lấy invitations có status = Pending
   - Include Project và Sender information

**Entities:**
7. **InvitationJoiningProject.cs** - Entity với factory method
   - `Create()`: Factory method để tạo invitation mới
   - Lưu RecipientEmail (không cần UserId)

**Infrastructure:**
8. **Startup.cs** - Đăng ký EmailService vào DI container

**API:**
9. **ProjectController.cs** - Controller với các endpoints mới
   - POST /projects/{projectId}/invite
   - GET /projects/invitations
   - POST /projects/invitations/{invitationId}/accept
   - POST /projects/invitations/{invitationId}/reject

## User Flow cho Frontend

### Flow 1: User đã có tài khoản

```
1. User login
2. Navigate to "Invitations" page
3. Call GET /api/v1/projects/invitations
4. Display list of pending invitations
5. User clicks "Accept" or "Reject"
6. Call POST /api/v1/projects/invitations/{id}/accept hoặc /reject
7. Refresh invitation list
8. If accepted, user can now access the project
```

### Flow 2: User chưa có tài khoản

```
1. User receives email
2. User clicks registration link
3. User registers with the SAME email as in invitation
4. User logs in
5. Navigate to "Invitations" page
6. Call GET /api/v1/projects/invitations (sẽ tự động tìm invitations cho email này)
7. Display list of pending invitations
8. User clicks "Accept"
9. User is added to project
```

### Recommended Frontend Implementation

```typescript
// 1. Check for pending invitations after login
useEffect(() => {
  if (isAuthenticated) {
    checkPendingInvitations();
  }
}, [isAuthenticated]);

async function checkPendingInvitations() {
  const invitations = await api.get('/api/v1/projects/invitations');
  if (invitations.length > 0) {
    // Show notification or redirect to invitations page
    showNotification(`You have ${invitations.length} pending invitation(s)`);
  }
}

// 2. Accept invitation
async function acceptInvitation(invitationId: string) {
  try {
    await api.post(`/api/v1/projects/invitations/${invitationId}/accept`);
    showSuccess('Invitation accepted! You can now access the project.');
    refreshInvitations();
    refreshProjectList();
  } catch (error) {
    showError(error.message);
  }
}
```

## Best Practices

### 1. Security

- Luôn sử dụng App Password thay vì mật khẩu thật
- Không commit credentials vào source control
- Sử dụng environment variables hoặc Azure Key Vault cho production
- Validate email ownership trước khi accept invitation

### 2. Error Handling

- EmailService catch và log exceptions
- Không throw exception nếu email fail (để không ảnh hưởng business logic)
- Trả về boolean để indicate success/failure
- Frontend nên xử lý các error cases:
  - Invitation already processed
  - User already member
  - Invitation not for current user's email

### 3. Performance

- Email được gửi async
- Có thể consider sử dụng background job (Hangfire) cho production để tránh block request
- Cache danh sách invitations ở client side
- Chỉ refresh khi cần thiết

### 4. User Experience

- Hiển thị số lượng pending invitations ở header/navigation
- Tự động check invitations sau khi login
- Hiển thị thông tin project name, sender trong invitation list
- Confirm dialog trước khi reject invitation
- Redirect to project page sau khi accept

## Troubleshooting

### Email không được gửi

1. Kiểm tra SMTP credentials trong appsettings.json
2. Kiểm tra logs để xem error message
3. Verify rằng Gmail App Password đã được tạo đúng
4. Kiểm tra firewall/network có block port 587 không
5. Test với email service online như Mailtrap.io

### Invitation không hiển thị sau khi đăng ký

**Nguyên nhân**: User đăng ký với email KHÁC với email được mời

**Giải pháp**:
- Frontend nên pre-fill email khi user click registration link từ invitation email
- Hiển thị warning nếu user thay đổi email trong registration form
- Có thể pass email qua query parameter: `/register?email=invited@example.com`

### User đã là member nhưng vẫn thấy invitation

**Nguyên nhân**: Invitation status không được update khi user được thêm vào project thông qua cách khác

**Giải pháp**:
- Khi accept invitation, có validation để check existing membership
- Có thể tạo background job để cleanup các invitations cũ
- Frontend filter out các invitations cho projects mà user đã là member

### Invitation đã được xử lý

**Error**: "This invitation has already been accepted/rejected"

**Giải pháp**:
- Frontend nên refresh invitation list sau mỗi action
- Disable accept/reject buttons sau khi click
- Hiển thị status của invitation rõ ràng

## Testing Scenarios

### 1. Test với user chưa có tài khoản

```bash
# 1. Invite user
POST /api/v1/projects/{projectId}/invite
{
  "recipientEmail": "newuser@example.com"
}

# 2. Register new account với ĐÚNG email
POST /api/v1/auth/register
{
  "email": "newuser@example.com",
  "password": "Password123!",
  ...
}

# 3. Login
POST /api/v1/auth/login

# 4. Get invitations (should return the invitation)
GET /api/v1/projects/invitations

# 5. Accept invitation
POST /api/v1/projects/invitations/{invitationId}/accept

# 6. Verify membership
GET /api/v1/users/my-projects
```

### 2. Test validation

```bash
# Try to accept someone else's invitation
# Should return: "This invitation is not for your email address"

# Try to accept already processed invitation  
# Should return: "This invitation has already been accepted"

# Try to accept when already member
# Should return: "You are already a member of this project"
```

## Production Checklist

- [ ] Cấu hình SMTP credentials đúng
- [ ] Set FrontendUrl đúng với production domain
- [ ] Test gửi email thực tế
- [ ] Implement rate limiting cho invite endpoint
- [ ] Set up email monitoring/logging
- [ ] Configure email templates với branding
- [ ] Test với các email providers khác nhau (Gmail, Outlook, etc.)
- [ ] Implement invitation expiration (optional)
- [ ] Add notification system cho pending invitations
- [ ] Set up error tracking cho email failures
