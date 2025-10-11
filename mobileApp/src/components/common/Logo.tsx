import React from 'react';
import { View, Text, StyleSheet } from 'react-native';
import { colors } from '../constants/theme';

interface LogoProps {
  size?: number;
}

const Logo: React.FC<LogoProps> = ({ size = 120 }) => {
  return (
    <View style={[styles.container, { width: size, height: size }]}>
      <View style={styles.logoCircle}>
        <Text style={styles.logoText}>CB</Text>
      </View>
      <Text style={styles.companyName}>Cookie Barrel</Text>
    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    alignItems: 'center',
    justifyContent: 'center',
  },
  logoCircle: {
    width: 100,
    height: 100,
    borderRadius: 50,
    backgroundColor: colors.primary,
    alignItems: 'center',
    justifyContent: 'center',
    shadowColor: '#000',
    shadowOffset: {
      width: 0,
      height: 4,
    },
    shadowOpacity: 0.3,
    shadowRadius: 4.65,
    elevation: 8,
  },
  logoText: {
    fontSize: 48,
    fontWeight: 'bold',
    color: '#fff',
    fontFamily: 'serif',
  },
  companyName: {
    marginTop: 8,
    fontSize: 14,
    fontWeight: '600',
    color: colors.primary,
    letterSpacing: 1,
  },
});

export default Logo;
