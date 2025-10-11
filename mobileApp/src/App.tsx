import React from 'react';
import { Provider } from 'react-redux';
import { StatusBar } from 'react-native';
import { PaperProvider } from 'react-native-paper';
import { store } from './store/store';
import AppNavigator from './navigation/AppNavigator';
import { colors } from './constants/theme';

const App = () => {
  return (
    <Provider store={store}>
      <PaperProvider>
        <StatusBar
          barStyle="light-content"
          backgroundColor={colors.primary}
        />
        <AppNavigator />
      </PaperProvider>
    </Provider>
  );
};

export default App;
