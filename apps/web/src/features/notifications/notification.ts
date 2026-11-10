export type SignalLabNotificationType =
  | "OrganizationInvitationReceived";

export type SignalLabNotificationTargetType =
  | "OrganizationInvitation";

export type SignalLabNotification = Readonly<
{
  id: string;
  type: SignalLabNotificationType;
  targetType: SignalLabNotificationTargetType;
  targetId: string;
  createdAt: string;
  readAt: string | null;
}>;

export type UnreadNotificationCount = Readonly<
{
  count: number;
}>;