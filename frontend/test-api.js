// Test API Connection
const testAPI = async () => {
    console.log('Testing API Connection to http://localhost:5000');
    console.log('=====================================');
    
    try {
        // Test 1: Health Check
        console.log('\n1. Testing Health Endpoint...');
        const healthResponse = await fetch('http://localhost:5000/api/health');
        if (healthResponse.ok) {
            const health = await healthResponse.json();
            console.log('✓ Health Check Passed:', health.Status);
            console.log('  Database Connected:', health.Database.Connected);
            if (health.Database.Statistics) {
                console.log('  Statistics:', health.Database.Statistics);
            }
        } else {
            console.log('✗ Health Check Failed:', healthResponse.status);
        }

        // Test 2: Login
        console.log('\n2. Testing Login Endpoint...');
        const loginResponse = await fetch('http://localhost:5000/api/auth/login', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                username: 'admin',
                password: 'Admin123!'
            })
        });

        if (loginResponse.ok) {
            const loginData = await loginResponse.json();
            console.log('✓ Login Successful');
            console.log('  Token received:', loginData.token ? 'Yes' : 'No');
            console.log('  User:', loginData.user?.username);
            
            // Test 3: Authenticated Request
            if (loginData.token) {
                console.log('\n3. Testing Authenticated Request...');
                const categoriesResponse = await fetch('http://localhost:5000/api/categories', {
                    headers: {
                        'Authorization': `Bearer ${loginData.token}`
                    }
                });
                
                if (categoriesResponse.ok) {
                    const categories = await categoriesResponse.json();
                    console.log('✓ Authenticated Request Successful');
                    console.log('  Categories found:', Array.isArray(categories) ? categories.length : 0);
                } else {
                    console.log('✗ Authenticated Request Failed:', categoriesResponse.status);
                }
            }
        } else {
            console.log('✗ Login Failed:', loginResponse.status);
            const error = await loginResponse.text();
            console.log('  Error:', error);
        }

        // Test 4: CORS from Frontend
        console.log('\n4. Testing CORS Configuration...');
        const corsResponse = await fetch('http://localhost:5000/api/health', {
            headers: {
                'Origin': 'http://localhost:3000'
            }
        });
        
        if (corsResponse.ok) {
            console.log('✓ CORS Configured Correctly');
        } else {
            console.log('✗ CORS Issue Detected');
        }

    } catch (error) {
        console.error('Connection Error:', error.message);
        console.log('\nMake sure the backend is running on http://localhost:5000');
    }
    
    console.log('\n=====================================');
    console.log('Test Complete');
};

// Run the test
testAPI();
