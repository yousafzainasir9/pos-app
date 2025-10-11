export const formatCurrency = (amount: number): string => {
  return `$${amount.toFixed(2)}`;
};

export const calculateGST = (amount: number, rate: number = 0.1): number => {
  return amount * rate;
};

export const calculateTotal = (subtotal: number, gstRate: number = 0.1): number => {
  return subtotal + calculateGST(subtotal, gstRate);
};
