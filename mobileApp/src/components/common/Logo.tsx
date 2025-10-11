import React from 'react';
import { View, Text, StyleSheet } from 'react-native';

interface LogoProps {
  size?: number;
}

const Logo: React.FC<LogoProps> = ({ size = 120 }) => {
  const logoCircleSize = size * 0.83; // 83% of container size
  
  return (
    <View style={[styles.container, { width: size * 1.2 }]}>
      <View style={[styles.logoCircle, { width: logoCircleSize, height: logoCircleSize, borderRadius: logoCircleSize / 2 }]}>
        <Text style={[styles.logoText, { fontSize: logoCircleSize * 0.48 }]}>CB</Text>
      </View>
      <Text style={styles.companyName}>Cookie Barrel</Text>
    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    alignItems: 'center',
    justifyContent: 'center',
    marginBottom: 8,
  },
  logoCircle: {
    backgroundColor: '#D97706', // Cookie Barrel orange
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
    fontWeight: 'bold',
    color: '#fff',
    fontFamily: 'serif',
  },
  companyName: {
    marginTop: 10,
    fontSize: 14,
    fontWeight: '600',
    color: '#D97706',
    letterSpacing: 1,
    textAlign: 'center',
  },
});

export default Logo;
