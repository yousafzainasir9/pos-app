import React from 'react';
import { View, Text, StyleSheet, TouchableOpacity } from 'react-native';
import { useDispatch } from 'react-redux';
import { setGuestMode } from '../store/slices/authSlice';
import { colors, spacing } from '../constants/theme';

const LoginScreen = () => {
  const dispatch = useDispatch();

  const handleGuestLogin = () => {
    // For now, just set guest mode with placeholder data
    dispatch(setGuestMode({ name: 'Guest', phone: '0400000000' }));
  };

  return (
    <View style={styles.container}>
      <Text style={styles.title}>Cookie Barrel</Text>
      <Text style={styles.subtitle}>Welcome to Mobile Ordering</Text>
      
      <TouchableOpacity style={styles.button} onPress={handleGuestLogin}>
        <Text style={styles.buttonText}>Continue as Guest</Text>
      </TouchableOpacity>
      
      <Text style={styles.note}>Login functionality coming soon...</Text>
    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    backgroundColor: colors.background,
    padding: spacing.lg,
  },
  title: {
    fontSize: 32,
    fontWeight: 'bold',
    color: colors.primary,
    marginBottom: spacing.sm,
  },
  subtitle: {
    fontSize: 18,
    color: colors.textLight,
    marginBottom: spacing.xl,
  },
  button: {
    backgroundColor: colors.primary,
    paddingHorizontal: spacing.xl,
    paddingVertical: spacing.md,
    borderRadius: 8,
    marginTop: spacing.lg,
  },
  buttonText: {
    color: '#fff',
    fontSize: 16,
    fontWeight: 'bold',
  },
  note: {
    marginTop: spacing.lg,
    color: colors.textLight,
    fontSize: 14,
  },
});

export default LoginScreen;
