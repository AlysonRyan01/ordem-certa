export type NotificationType = 'BudgetCreated' | 'BudgetApproved' | 'BudgetRefused' | 'ReadyForPickup';
export type NotificationRecipientType = 'Customer' | 'Company';

export interface ServiceOrderNotificationOutput {
  id: string;
  type: NotificationType;
  recipientType: NotificationRecipientType;
  recipientName: string;
  phone: string;
  sentAt: string;
}
