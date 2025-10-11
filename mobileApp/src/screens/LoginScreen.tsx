import React, { useState } from 'react';
import {
  View,
  Text,
  StyleSheet,
  TouchableOpacity,
  TextInput,
  ActivityIndicator,
  KeyboardAvoidingView,
  Platform,
  ScrollView,
  Alert,
} from 'react-native';
import { useDispatch, useSelector } from 'react-redux';
import { loginUser, setGuestMode, clearError } from '../store/slices/authSlice';
import { colors, spacing } from '../constants/theme';
import { AppDispatch, RootState } from '../store/store';
import Logo from '../components/common/Logo';

type LoginMode = 'username' | 'pin';

const LoginScreen = () => {
  const dispatch = useDispatch<AppDispatch>();
  const { isLoading, error } = useSelector((state: RootState) => state.auth);
  
  const [loginMode, setLoginMode] = useState<LoginMode>('username');
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [pin, setPin] = useState('');

  const handleUsernameLogin = async () => {
    if (!username.trim() || !password.trim()) {
      Alert.alert('Error', 'Please enter both username and password');
      return;
    }

    try {
      await dispatch(loginUser({ username, password })).unwrap();
    } catch (err) {
      // Error is handled by the slice and displayed below
    }
  };

  const handlePinLogin = async () => {
    if (!pin.trim() || pin.length !== 4) {
      Alert.alert('Error', 'Please enter a 4-digit PIN');
      return;
    }

    // Use PIN as username/password for customer login
    // This works because customers have their PIN stored
    try {
      await dispatch(loginUser({ username: pin, password: pin })).unwrap();
    } catch (err) {
      // Error is handled by the slice and displayed below
    }
  };

  const handleGuestLogin = () => {
    dispatch(setGuestMode({ name: 'Guest', phone: '0400000000' }));
  };

  const clearErrors = () => {
    dispatch(clearError());
  };

  const switchMode = (mode: LoginMode) => {
    setLoginMode(mode);
    clearErrors();
    // Clear inputs when switching
    setUsername('');
    setPassword('');
    setPin('');
  };

  return (
    <KeyboardAvoidingView
      style={styles.container}
      behavior={Platform.OS === 'ios' ? 'padding' : 'height'}
    >
      <ScrollView
        contentContainerStyle={styles.scrollContent}
        keyboardShouldPersistTaps="handled"
      >
        {/* Logo Section */}
        <View style={styles.header}>
          <Logo size={140} />
          <Text style={styles.subtitle}>Mobile Ordering</Text>
        </View>

        {/* Login Mode Toggle */}
        <View style={styles.modeToggle}>
          <TouchableOpacity
            style={[
              styles.modeButton,
              loginMode === 'username' && styles.modeButtonActive,
            ]}
            onPress={() => switchMode('username')}
            disabled={isLoading}
          >
            <Text
              style={[
                styles.modeButtonText,
                loginMode === 'username' && styles.modeButtonTextActive,
              ]}
            >
              Username
            </Text>
          </TouchableOpacity>
          <TouchableOpacity
            style={[
              styles.modeButton,
              loginMode === 'pin' && styles.modeButtonActive,
            ]}
            onPress={() => switchMode('pin')}
            disabled={isLoading}
          >
            <Text
              style={[
                styles.modeButtonText,
                loginMode === 'pin' && styles.modeButtonTextActive,
              ]}
            >
              PIN
            </Text>
          </TouchableOpacity>
        </View>

        {/* Form Section */}
        <View style={styles.formContainer}>
          {loginMode === 'username' ? (
            // Username/Password Login
            <>
              <Text style={styles.label}>Username</Text>
              <TextInput
                style={styles.input}
                placeholder="Enter your username"
                placeholderTextColor={colors.textLight}
                value={username}
                onChangeText={(text) => {
                  setUsername(text);
                  clearErrors();
                }}
                autoCapitalize="none"
                autoCorrect={false}
                editable={!isLoading}
              />

              <Text style={styles.label}>Password</Text>
              <TextInput
                style={styles.input}
                placeholder="Enter your password"
                placeholderTextColor={colors.textLight}
                value={password}
                onChangeText={(text) => {
                  setPassword(text);
                  clearErrors();
                }}
                secureTextEntry
                autoCapitalize="none"
                autoCorrect={false}
                editable={!isLoading}
              />

              <TouchableOpacity
                style={[styles.button, styles.loginButton, isLoading && styles.buttonDisabled]}
                onPress={handleUsernameLogin}
                disabled={isLoading}
              >
                {isLoading ? (
                  <ActivityIndicator color="#fff" />
                ) : (
                  <Text style={styles.buttonText}>Login</Text>
                )}
              </TouchableOpacity>

              <View style={styles.helpContainer}>
                <Text style={styles.helpTitle}>Test Credentials:</Text>
                <Text style={styles.helpText}>Username: customer</Text>
                <Text style={styles.helpText}>Password: Customer123!</Text>
              </View>
            </>
          ) : (
            // PIN Login
            <>
              <Text style={styles.label}>Enter PIN</Text>
              <TextInput
                style={styles.pinInput}
                placeholder="••••"
                placeholderTextColor={colors.textLight}
                value={pin}
                onChangeText={(text) => {
                  // Only allow numbers and max 4 digits
                  if (/^\d{0,4}$/.test(text)) {
                    setPin(text);
                    clearErrors();
                  }
                }}
                keyboardType="numeric"
                maxLength={4}
                secureTextEntry
                editable={!isLoading}
                textAlign="center"
              />

              <TouchableOpacity
                style={[styles.button, styles.loginButton, isLoading && styles.buttonDisabled]}
                onPress={handlePinLogin}
                disabled={isLoading}
              >
                {isLoading ? (
                  <ActivityIndicator color="#fff" />
                ) : (
                  <Text style={styles.buttonText}>Login with PIN</Text>
                )}
              </TouchableOpacity>

              <View style={styles.helpContainer}>
                <Text style={styles.helpTitle}>Quick PIN Access:</Text>
                <Text style={styles.helpText}>Test PIN: 1234</Text>
                <Text style={styles.helpText}>Fast and secure login</Text>
              </View>
            </>
          )}

          {error && (
            <View style={styles.errorContainer}>
              <Text style={styles.errorText}>{error}</Text>
            </View>
          )}

          <View style={styles.divider}>
            <View style={styles.dividerLine} />
            <Text style={styles.dividerText}>OR</Text>
            <View style={styles.dividerLine} />
          </View>

          <TouchableOpacity
            style={[styles.button, styles.guestButton]}
            onPress={handleGuestLogin}
            disabled={isLoading}
          >
            <Text style={styles.guestButtonText}>Continue as Guest</Text>
          </TouchableOpacity>
        </View>
      </ScrollView>
    </KeyboardAvoidingView>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: colors.background,
  },
  scrollContent: {
    flexGrow: 1,
    justifyContent: 'center',
    padding: spacing.lg,
  },
  header: {
    alignItems: 'center',
    marginBottom: spacing.xl,
  },
  subtitle: {
    fontSize: 18,
    color: colors.textLight,
    marginTop: spacing.md,
  },
  modeToggle: {
    flexDirection: 'row',
    backgroundColor: '#f3f4f6',
    borderRadius: 8,
    padding: 4,
    marginBottom: spacing.lg,
  },
  modeButton: {
    flex: 1,
    paddingVertical: spacing.sm,
    alignItems: 'center',
    borderRadius: 6,
  },
  modeButtonActive: {
    backgroundColor: '#fff',
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 1 },
    shadowOpacity: 0.1,
    shadowRadius: 2,
    elevation: 2,
  },
  modeButtonText: {
    fontSize: 15,
    fontWeight: '600',
    color: colors.textLight,
  },
  modeButtonTextActive: {
    color: colors.primary,
  },
  formContainer: {
    width: '100%',
  },
  label: {
    fontSize: 16,
    fontWeight: '600',
    color: colors.text,
    marginBottom: spacing.xs,
    marginTop: spacing.md,
  },
  input: {
    backgroundColor: '#fff',
    borderWidth: 1,
    borderColor: colors.border,
    borderRadius: 8,
    paddingHorizontal: spacing.md,
    paddingVertical: spacing.sm,
    fontSize: 16,
    color: colors.text,
  },
  pinInput: {
    backgroundColor: '#fff',
    borderWidth: 2,
    borderColor: colors.primary,
    borderRadius: 12,
    paddingHorizontal: spacing.md,
    paddingVertical: spacing.lg,
    fontSize: 32,
    color: colors.text,
    fontWeight: 'bold',
    letterSpacing: 16,
    textAlign: 'center',
  },
  errorContainer: {
    backgroundColor: '#fee',
    borderRadius: 8,
    padding: spacing.sm,
    marginTop: spacing.md,
  },
  errorText: {
    color: colors.error,
    fontSize: 14,
    textAlign: 'center',
  },
  button: {
    borderRadius: 8,
    paddingVertical: spacing.md,
    alignItems: 'center',
    justifyContent: 'center',
    marginTop: spacing.lg,
  },
  loginButton: {
    backgroundColor: colors.primary,
  },
  guestButton: {
    backgroundColor: '#fff',
    borderWidth: 1,
    borderColor: colors.primary,
  },
  buttonDisabled: {
    opacity: 0.6,
  },
  buttonText: {
    color: '#fff',
    fontSize: 16,
    fontWeight: 'bold',
  },
  guestButtonText: {
    color: colors.primary,
    fontSize: 16,
    fontWeight: 'bold',
  },
  divider: {
    flexDirection: 'row',
    alignItems: 'center',
    marginTop: spacing.lg,
  },
  dividerLine: {
    flex: 1,
    height: 1,
    backgroundColor: colors.border,
  },
  dividerText: {
    marginHorizontal: spacing.md,
    color: colors.textLight,
    fontSize: 14,
  },
  helpContainer: {
    marginTop: spacing.md,
    padding: spacing.md,
    backgroundColor: '#f0f9ff',
    borderRadius: 8,
    borderWidth: 1,
    borderColor: '#bfdbfe',
  },
  helpTitle: {
    fontSize: 14,
    fontWeight: '600',
    color: '#1e40af',
    textAlign: 'center',
    marginBottom: spacing.xs,
  },
  helpText: {
    fontSize: 12,
    color: '#1e40af',
    textAlign: 'center',
    marginBottom: 2,
  },
});

export default LoginScreen;
