import { z } from 'zod';

export const loginSchema = z.object({
  username: z.string().min(1, 'Username is required'),
  password: z.string().min(1, 'Password is required')
});

export const pinLoginSchema = z.object({
  pin: z.string().length(4, 'PIN must be 4 digits').regex(/^\d+$/, 'PIN must contain only digits'),
  storeId: z.number().positive('Store ID is required')
});

export const openShiftSchema = z.object({
  startingCash: z
    .number({ required_error: 'Starting cash is required' })
    .min(0, 'Starting cash cannot be negative'),
  notes: z.string().optional()
});

export const closeShiftSchema = z.object({
  endingCash: z
    .number({ required_error: 'Ending cash is required' })
    .min(0, 'Ending cash cannot be negative'),
  notes: z.string().optional()
});

export const paymentSchema = z.object({
  amount: z
    .number({ required_error: 'Amount is required' })
    .positive('Amount must be greater than 0'),
  paymentMethod: z.number().min(1).max(7),
  referenceNumber: z.string().optional(),
  cardLastFourDigits: z
    .string()
    .length(4, 'Last 4 digits required')
    .regex(/^\d+$/, 'Must be digits')
    .optional()
    .or(z.literal('')),
  cardType: z.string().optional(),
  notes: z.string().optional()
});

export const customerSchema = z.object({
  firstName: z.string().min(1, 'First name is required'),
  lastName: z.string().min(1, 'Last name is required'),
  email: z.string().email('Invalid email').optional().or(z.literal('')),
  phone: z.string().optional(),
  address: z.string().optional(),
  city: z.string().optional(),
  state: z.string().optional(),
  postalCode: z.string().optional(),
  country: z.string().optional(),
  loyaltyCardNumber: z.string().optional()
});

export const productSearchSchema = z.object({
  search: z.string().optional(),
  categoryId: z.number().optional(),
  subcategoryId: z.number().optional(),
  barcode: z.string().optional()
});

export type LoginFormData = z.infer<typeof loginSchema>;
export type PinLoginFormData = z.infer<typeof pinLoginSchema>;
export type OpenShiftFormData = z.infer<typeof openShiftSchema>;
export type CloseShiftFormData = z.infer<typeof closeShiftSchema>;
export type PaymentFormData = z.infer<typeof paymentSchema>;
export type CustomerFormData = z.infer<typeof customerSchema>;
export type ProductSearchFormData = z.infer<typeof productSearchSchema>;
