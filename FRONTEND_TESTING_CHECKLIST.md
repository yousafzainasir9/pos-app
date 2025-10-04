# 🧪 Frontend Testing Checklist

## Quick Start
1. Make sure backend is running: `https://localhost:7021`
2. Run: `test-frontend.bat` (or `cd frontend && npm run dev`)
3. Open: `http://localhost:5173`

---

## ✅ Test Scenarios

### 1. Valid Login Test
**Steps:**
1. Navigate to login page
2. Enter username: `admin`
3. Enter password: `Admin123!`
4. Click "Login"

**Expected Result:**
- ✅ Shows "Login successful!" toast notification
- ✅ Redirects to dashboard/home page
- ✅ User information displayed in header
- ✅ Token saved in localStorage
- ✅ No console errors

**Status:** [ ] Pass [ ] Fail

---

### 2. Invalid Credentials Test
**Steps:**
1. Navigate to login page
2. Enter username: `wronguser`
3. Enter password: `wrongpass`
4. Click "Login"

**Expected Result:**
- ✅ Shows error toast with specific message (e.g., "Invalid username or password")
- ✅ Stays on login page
- ✅ Username/password fields remain visible
- ✅ No console errors
- ✅ No redirect

**Status:** [ ] Pass [ ] Fail

---

### 3. Validation Errors Test
**Steps:**
1. Navigate to login page
2. Leave username field empty
3. Leave password field empty
4. Click "Login"

**Expected Result:**
- ✅ Shows validation error toast for username: "Username: Username is required"
- ✅ Shows validation error toast for password: "Password: Password is required"
- ✅ Both errors visible simultaneously
- ✅ Stays on login page
- ✅ No console errors

**Additional Test:**
1. Enter username: `admin`
2. Enter password: `123` (too short)
3. Click "Login"

**Expected Result:**
- ✅ Shows error about password length
- ✅ Error message is clear and helpful

**Status:** [ ] Pass [ ] Fail

---

### 4. PIN Login Test
**Steps:**
1. Navigate to login page
2. Switch to "PIN Login" tab
3. Enter PIN: `9999`
4. Select Store: `Main Store` (or first available)
5. Click "Login"

**Expected Result:**
- ✅ Shows "Login successful!" toast
- ✅ Redirects to appropriate page
- ✅ User logged in successfully
- ✅ Token saved in localStorage
- ✅ No console errors

**Status:** [ ] Pass [ ] Fail

---

### 5. Rate Limiting Test
**Steps:**
1. Navigate to login page
2. Enter wrong credentials 5 times in quick succession
3. Try logging in a 6th time

**Expected Result:**
- ✅ After 5th attempt, shows rate limit warning
- ✅ 6th attempt blocked with message: "Too many requests. Please try again later."
- ✅ HTTP 429 status code in network tab
- ✅ Clear error message to user
- ✅ No console errors

**Status:** [ ] Pass [ ] Fail

---

### 6. Token Refresh Test
**Steps:**
1. Login successfully
2. Wait for token to near expiry (or manually edit localStorage token to be near expiry)
3. Navigate to a protected page or make an API call
4. Check network tab for refresh token request

**Expected Result:**
- ✅ Token automatically refreshes in background
- ✅ New token saved to localStorage
- ✅ Original request succeeds after refresh
- ✅ No logout or redirect to login
- ✅ User stays authenticated seamlessly
- ✅ No visible interruption to user

**Status:** [ ] Pass [ ] Fail

---

### 7. Logout Test
**Steps:**
1. Login successfully
2. Click logout button (usually in header/menu)
3. Check localStorage

**Expected Result:**
- ✅ Shows "Logged out successfully" toast
- ✅ Redirects to login page
- ✅ Token removed from localStorage
- ✅ User info removed from localStorage
- ✅ Refresh token removed from localStorage
- ✅ Cannot access protected pages
- ✅ No console errors

**Status:** [ ] Pass [ ] Fail

---

### 8. Protected Routes Test
**Steps:**
1. Without logging in, try to access:
   - `/admin`
   - `/pos`
   - `/orders`
   - `/reports`

**Expected Result:**
- ✅ All protected routes redirect to login
- ✅ Cannot access without authentication
- ✅ After login, can access appropriate routes based on role

**Status:** [ ] Pass [ ] Fail

---

### 9. API Error Handling Test
**Steps:**
1. Login successfully
2. Stop the backend server
3. Try to load products or make an API call
4. Restart backend
5. Try again

**Expected Result:**
- ✅ Shows appropriate error message when backend down
- ✅ Doesn't crash the frontend
- ✅ Gracefully handles network errors
- ✅ Works normally when backend is back up
- ✅ No unhandled promise rejections

**Status:** [ ] Pass [ ] Fail

---

### 10. Session Persistence Test
**Steps:**
1. Login successfully
2. Navigate to different pages
3. Refresh the browser (F5)
4. Check if still logged in

**Expected Result:**
- ✅ User remains logged in after refresh
- ✅ Token persists in localStorage
- ✅ User info restored correctly
- ✅ Can continue using app without re-login
- ✅ Protected routes still accessible

**Status:** [ ] Pass [ ] Fail

---

### 11. Multiple Tab Test
**Steps:**
1. Login in Tab 1
2. Open app in Tab 2
3. Check if logged in
4. Logout in Tab 1
5. Check Tab 2

**Expected Result:**
- ✅ Both tabs share authentication state
- ✅ Login in one tab applies to both
- ✅ Logout in one tab logs out both (ideally)
- ✅ No conflicts between tabs

**Status:** [ ] Pass [ ] Fail

---

### 12. Build Production Test
**Steps:**
```bash
cd frontend
npm run build
npm run preview
```

**Expected Result:**
- ✅ Build completes without errors
- ✅ No TypeScript errors
- ✅ Production preview works
- ✅ All features work in production build
- ✅ No console errors in production

**Status:** [ ] Pass [ ] Fail

---

## Browser Console Checks

### Things to Check in Console:
- [ ] No red errors
- [ ] No unhandled promise rejections
- [ ] No 404s (except expected missing resources)
- [ ] API calls show correct request/response format
- [ ] Tokens are being set correctly

### Network Tab Checks:
- [ ] `/api/auth/login` returns 200 for valid credentials
- [ ] `/api/auth/login` returns 400/401 for invalid credentials
- [ ] Response format matches: `{ success, data, error }`
- [ ] Error responses have proper structure
- [ ] Token refresh endpoint works
- [ ] Authorization headers present on protected requests

---

## localStorage Verification

### After Successful Login, localStorage should have:
```javascript
// Check in browser console:
console.log('Token:', localStorage.getItem('token'));
console.log('Refresh Token:', localStorage.getItem('refreshToken'));
console.log('User:', JSON.parse(localStorage.getItem('user')));
```

**Expected:**
- ✅ `token` - JWT token string
- ✅ `refreshToken` - Refresh token string
- ✅ `user` - JSON object with user info

### After Logout, localStorage should be:
- ✅ No `token`
- ✅ No `refreshToken`
- ✅ No `user`

---

## Edge Cases to Test

### 1. Empty Input Fields
- [ ] Empty username
- [ ] Empty password
- [ ] Empty PIN
- [ ] No store selected (PIN login)

### 2. Special Characters
- [ ] Username with spaces
- [ ] Password with special chars
- [ ] SQL injection attempts (should be safe)
- [ ] XSS attempts (should be safe)

### 3. Network Issues
- [ ] Slow connection
- [ ] Backend timeout
- [ ] Backend returns 500 error
- [ ] Backend returns malformed JSON

### 4. Token Issues
- [ ] Expired token
- [ ] Invalid token
- [ ] Missing token
- [ ] Malformed token

---

## Performance Checks

### Page Load Times:
- [ ] Login page loads < 2 seconds
- [ ] Dashboard loads < 3 seconds
- [ ] No unnecessary API calls
- [ ] No memory leaks (check over time)

### API Response Times:
- [ ] Login responds < 1 second
- [ ] Token refresh < 500ms
- [ ] Protected routes < 2 seconds

---

## Accessibility Checks

- [ ] Can navigate login form with keyboard (Tab)
- [ ] Can submit form with Enter key
- [ ] Error messages are readable
- [ ] Form labels properly associated
- [ ] Focus indicators visible
- [ ] Color contrast sufficient

---

## Mobile/Responsive Checks

- [ ] Login page responsive on mobile
- [ ] Touch targets appropriate size
- [ ] No horizontal scrolling
- [ ] Toast notifications visible
- [ ] Keyboard doesn't obscure inputs

---

## Final Verification

### Before Declaring Success:
1. [ ] All 12 test scenarios pass
2. [ ] No console errors in any scenario
3. [ ] localStorage managed correctly
4. [ ] Network requests show correct format
5. [ ] Production build works
6. [ ] All edge cases handled
7. [ ] Performance acceptable
8. [ ] Accessibility considerations met

---

## Test Results Summary

**Date Tested:** _________________  
**Tester:** _________________  
**Environment:** Development / Production  

**Tests Passed:** ___ / 12  
**Critical Issues:** _________________  
**Minor Issues:** _________________  

**Overall Status:** [ ] ✅ Ready for Production [ ] ⚠️ Needs Work [ ] ❌ Major Issues

---

## Notes

Use this space to document any issues found:

```
Issue 1:
- Description:
- Steps to reproduce:
- Expected:
- Actual:
- Severity: Critical / High / Medium / Low

Issue 2:
- Description:
- Steps to reproduce:
- Expected:
- Actual:
- Severity: Critical / High / Medium / Low
```

---

## Quick Commands

```bash
# Start frontend only
cd frontend
npm run dev

# Build for production
npm run build

# Preview production build
npm run preview

# Check TypeScript errors
npm run build

# Clear and reinstall
rm -rf node_modules package-lock.json
npm install

# View localStorage (in browser console)
console.table({
  token: localStorage.getItem('token')?.substring(0, 20) + '...',
  hasRefreshToken: !!localStorage.getItem('refreshToken'),
  user: localStorage.getItem('user')
});
```

---

**Remember:** Test thoroughly before deploying to production! 🚀
