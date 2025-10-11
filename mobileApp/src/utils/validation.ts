export const validatePhone = (phone: string): boolean => {
  // Australian phone format: 04XX XXX XXX or 02/03/07/08 XXXX XXXX
  const phoneRegex = /^(?:\+?61|0)[2-478](?:[ -]?[0-9]){8}$/;
  return phoneRegex.test(phone.replace(/\s/g, ''));
};

export const formatPhone = (phone: string): string => {
  // Remove all non-digit characters
  const cleaned = phone.replace(/\D/g, '');
  
  // Format as 04XX XXX XXX for mobile
  if (cleaned.startsWith('04') && cleaned.length === 10) {
    return cleaned.replace(/(\d{4})(\d{3})(\d{3})/, '$1 $2 $3');
  }
  
  return phone;
};

export const validateName = (name: string): boolean => {
  return name.trim().length >= 2;
};
