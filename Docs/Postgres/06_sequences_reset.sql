-- Coolzo PostgreSQL Migration | File 6 | Reset Identity Sequences
-- Run after data is loaded so auto-increment starts after last inserted ID

SELECT setval(
  pg_get_serial_sequence('public."tblAMCPlan"', 'AmcPlanId'),
  COALESCE((SELECT MAX("AmcPlanId") FROM public."tblAMCPlan"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblAcType"', 'AcTypeId'),
  COALESCE((SELECT MAX("AcTypeId") FROM public."tblAcType"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblAuditLog"', 'AuditLogId'),
  COALESCE((SELECT MAX("AuditLogId") FROM public."tblAuditLog"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblBrand"', 'BrandId'),
  COALESCE((SELECT MAX("BrandId") FROM public."tblBrand"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblBusinessHourConfiguration"', 'BusinessHourConfigurationId'),
  COALESCE((SELECT MAX("BusinessHourConfigurationId") FROM public."tblBusinessHourConfiguration"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblCMSBanner"', 'CMSBannerId'),
  COALESCE((SELECT MAX("CMSBannerId") FROM public."tblCMSBanner"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblCMSBlock"', 'CMSBlockId'),
  COALESCE((SELECT MAX("CMSBlockId") FROM public."tblCMSBlock"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblCMSContentVersion"', 'CMSContentVersionId'),
  COALESCE((SELECT MAX("CMSContentVersionId") FROM public."tblCMSContentVersion"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblCMSFaq"', 'CMSFaqId'),
  COALESCE((SELECT MAX("CMSFaqId") FROM public."tblCMSFaq"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblCancellationPolicy"', 'CancellationPolicyId'),
  COALESCE((SELECT MAX("CancellationPolicyId") FROM public."tblCancellationPolicy"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblDiagnosisResultMaster"', 'DiagnosisResultMasterId'),
  COALESCE((SELECT MAX("DiagnosisResultMasterId") FROM public."tblDiagnosisResultMaster"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblDisplayContentSetting"', 'DisplayContentSettingId'),
  COALESCE((SELECT MAX("DisplayContentSettingId") FROM public."tblDisplayContentSetting"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblDynamicMasterRecord"', 'DynamicMasterRecordId'),
  COALESCE((SELECT MAX("DynamicMasterRecordId") FROM public."tblDynamicMasterRecord"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblFeatureFlag"', 'FeatureFlagId'),
  COALESCE((SELECT MAX("FeatureFlagId") FROM public."tblFeatureFlag"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblHelperTaskChecklist"', 'HelperTaskChecklistId'),
  COALESCE((SELECT MAX("HelperTaskChecklistId") FROM public."tblHelperTaskChecklist"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblHolidayConfiguration"', 'HolidayConfigurationId'),
  COALESCE((SELECT MAX("HolidayConfigurationId") FROM public."tblHolidayConfiguration"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblItemCategory"', 'ItemCategoryId'),
  COALESCE((SELECT MAX("ItemCategoryId") FROM public."tblItemCategory"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblLeadSource"', 'LeadSourceId'),
  COALESCE((SELECT MAX("LeadSourceId") FROM public."tblLeadSource"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblNotificationTemplate"', 'NotificationTemplateId'),
  COALESCE((SELECT MAX("NotificationTemplateId") FROM public."tblNotificationTemplate"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblNotificationTriggerConfiguration"', 'NotificationTriggerConfigurationId'),
  COALESCE((SELECT MAX("NotificationTriggerConfigurationId") FROM public."tblNotificationTriggerConfiguration"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblOfflineSyncQueueItem"', 'OfflineSyncQueueItemId'),
  COALESCE((SELECT MAX("OfflineSyncQueueItemId") FROM public."tblOfflineSyncQueueItem"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblPermission"', 'PermissionId'),
  COALESCE((SELECT MAX("PermissionId") FROM public."tblPermission"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblPricingModel"', 'PricingModelId'),
  COALESCE((SELECT MAX("PricingModelId") FROM public."tblPricingModel"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblRole"', 'RoleId'),
  COALESCE((SELECT MAX("RoleId") FROM public."tblRole"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblRolePermission"', 'RolePermissionId'),
  COALESCE((SELECT MAX("RolePermissionId") FROM public."tblRolePermission"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblServiceCategory"', 'ServiceCategoryId'),
  COALESCE((SELECT MAX("ServiceCategoryId") FROM public."tblServiceCategory"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblService"', 'ServiceId'),
  COALESCE((SELECT MAX("ServiceId") FROM public."tblService"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblComplaintIssueMaster"', 'ComplaintIssueMasterId'),
  COALESCE((SELECT MAX("ComplaintIssueMasterId") FROM public."tblComplaintIssueMaster"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblServiceChecklistMaster"', 'ServiceChecklistMasterId'),
  COALESCE((SELECT MAX("ServiceChecklistMasterId") FROM public."tblServiceChecklistMaster"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblSupplier"', 'SupplierId'),
  COALESCE((SELECT MAX("SupplierId") FROM public."tblSupplier"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblPurchaseOrder"', 'PurchaseOrderId'),
  COALESCE((SELECT MAX("PurchaseOrderId") FROM public."tblPurchaseOrder"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblSupportTicketCategory"', 'SupportTicketCategoryId'),
  COALESCE((SELECT MAX("SupportTicketCategoryId") FROM public."tblSupportTicketCategory"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblSupportTicketPriority"', 'SupportTicketPriorityId'),
  COALESCE((SELECT MAX("SupportTicketPriorityId") FROM public."tblSupportTicketPriority"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblSystemAlert"', 'SystemAlertId'),
  COALESCE((SELECT MAX("SystemAlertId") FROM public."tblSystemAlert"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblSystemConfiguration"', 'SystemConfigurationId'),
  COALESCE((SELECT MAX("SystemConfigurationId") FROM public."tblSystemConfiguration"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblSystemSetting"', 'SystemSettingId'),
  COALESCE((SELECT MAX("SystemSettingId") FROM public."tblSystemSetting"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblTonnage"', 'TonnageId'),
  COALESCE((SELECT MAX("TonnageId") FROM public."tblTonnage"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblUnitOfMeasure"', 'UnitOfMeasureId'),
  COALESCE((SELECT MAX("UnitOfMeasureId") FROM public."tblUnitOfMeasure"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblItem"', 'ItemId'),
  COALESCE((SELECT MAX("ItemId") FROM public."tblItem"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblItemRate"', 'ItemRateId'),
  COALESCE((SELECT MAX("ItemRateId") FROM public."tblItemRate"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblPurchaseOrderItem"', 'PurchaseOrderItemId'),
  COALESCE((SELECT MAX("PurchaseOrderItemId") FROM public."tblPurchaseOrderItem"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblUser"', 'UserId'),
  COALESCE((SELECT MAX("UserId") FROM public."tblUser"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblCustomer"', 'CustomerId'),
  COALESCE((SELECT MAX("CustomerId") FROM public."tblCustomer"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblCommunicationPreference"', 'CommunicationPreferenceId'),
  COALESCE((SELECT MAX("CommunicationPreferenceId") FROM public."tblCommunicationPreference"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblCustomerEquipment"', 'CustomerEquipmentId'),
  COALESCE((SELECT MAX("CustomerEquipmentId") FROM public."tblCustomerEquipment"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblHelperProfile"', 'HelperProfileId'),
  COALESCE((SELECT MAX("HelperProfileId") FROM public."tblHelperProfile"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblHelperAttendance"', 'HelperAttendanceId'),
  COALESCE((SELECT MAX("HelperAttendanceId") FROM public."tblHelperAttendance"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblOtpVerification"', 'OtpVerificationId'),
  COALESCE((SELECT MAX("OtpVerificationId") FROM public."tblOtpVerification"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblSupportTicket"', 'SupportTicketId'),
  COALESCE((SELECT MAX("SupportTicketId") FROM public."tblSupportTicket"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblSupportTicketAssignment"', 'SupportTicketAssignmentId'),
  COALESCE((SELECT MAX("SupportTicketAssignmentId") FROM public."tblSupportTicketAssignment"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblSupportTicketEscalation"', 'SupportTicketEscalationId'),
  COALESCE((SELECT MAX("SupportTicketEscalationId") FROM public."tblSupportTicketEscalation"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblSupportTicketLink"', 'SupportTicketLinkId'),
  COALESCE((SELECT MAX("SupportTicketLinkId") FROM public."tblSupportTicketLink"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblSupportTicketReply"', 'SupportTicketReplyId'),
  COALESCE((SELECT MAX("SupportTicketReplyId") FROM public."tblSupportTicketReply"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblSupportTicketStatusHistory"', 'SupportTicketStatusHistoryId'),
  COALESCE((SELECT MAX("SupportTicketStatusHistoryId") FROM public."tblSupportTicketStatusHistory"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblUserPasswordHistory"', 'UserPasswordHistoryId'),
  COALESCE((SELECT MAX("UserPasswordHistoryId") FROM public."tblUserPasswordHistory"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblUserRole"', 'UserRoleId'),
  COALESCE((SELECT MAX("UserRoleId") FROM public."tblUserRole"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblUserSession"', 'UserSessionId'),
  COALESCE((SELECT MAX("UserSessionId") FROM public."tblUserSession"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblRefreshToken"', 'RefreshTokenId'),
  COALESCE((SELECT MAX("RefreshTokenId") FROM public."tblRefreshToken"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblWarehouse"', 'WarehouseId'),
  COALESCE((SELECT MAX("WarehouseId") FROM public."tblWarehouse"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblWarehouseStock"', 'WarehouseStockId'),
  COALESCE((SELECT MAX("WarehouseStockId") FROM public."tblWarehouseStock"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblWarrantyRule"', 'WarrantyRuleId'),
  COALESCE((SELECT MAX("WarrantyRuleId") FROM public."tblWarrantyRule"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblWorkflowStatusHistory"', 'WorkflowStatusHistoryId'),
  COALESCE((SELECT MAX("WorkflowStatusHistoryId") FROM public."tblWorkflowStatusHistory"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblZone"', 'ZoneId'),
  COALESCE((SELECT MAX("ZoneId") FROM public."tblZone"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblCustomerAddress"', 'CustomerAddressId'),
  COALESCE((SELECT MAX("CustomerAddressId") FROM public."tblCustomerAddress"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblSlotConfiguration"', 'SlotConfigurationId'),
  COALESCE((SELECT MAX("SlotConfigurationId") FROM public."tblSlotConfiguration"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblSlotAvailability"', 'SlotAvailabilityId'),
  COALESCE((SELECT MAX("SlotAvailabilityId") FROM public."tblSlotAvailability"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblBooking"', 'BookingId'),
  COALESCE((SELECT MAX("BookingId") FROM public."tblBooking"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblBookingLine"', 'BookingLineId'),
  COALESCE((SELECT MAX("BookingLineId") FROM public."tblBookingLine"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblBookingStatusHistory"', 'BookingStatusHistoryId'),
  COALESCE((SELECT MAX("BookingStatusHistoryId") FROM public."tblBookingStatusHistory"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblCampaign"', 'CampaignId'),
  COALESCE((SELECT MAX("CampaignId") FROM public."tblCampaign"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblCustomerReview"', 'CustomerReviewId'),
  COALESCE((SELECT MAX("CustomerReviewId") FROM public."tblCustomerReview"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblServiceRequest"', 'ServiceRequestId'),
  COALESCE((SELECT MAX("ServiceRequestId") FROM public."tblServiceRequest"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblCancellationRecord"', 'CancellationRecordId'),
  COALESCE((SELECT MAX("CancellationRecordId") FROM public."tblCancellationRecord"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblJobCard"', 'JobCardId'),
  COALESCE((SELECT MAX("JobCardId") FROM public."tblJobCard"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblJobAttachment"', 'JobAttachmentId'),
  COALESCE((SELECT MAX("JobAttachmentId") FROM public."tblJobAttachment"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblJobChecklistResponse"', 'JobChecklistResponseId'),
  COALESCE((SELECT MAX("JobChecklistResponseId") FROM public."tblJobChecklistResponse"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblJobDiagnosis"', 'JobDiagnosisId'),
  COALESCE((SELECT MAX("JobDiagnosisId") FROM public."tblJobDiagnosis"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblJobExecutionNote"', 'JobExecutionNoteId'),
  COALESCE((SELECT MAX("JobExecutionNoteId") FROM public."tblJobExecutionNote"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblJobExecutionTimeline"', 'JobExecutionTimelineId'),
  COALESCE((SELECT MAX("JobExecutionTimelineId") FROM public."tblJobExecutionTimeline"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblLeads"', 'LeadId'),
  COALESCE((SELECT MAX("LeadId") FROM public."tblLeads"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblLeadAssignment"', 'LeadAssignmentId'),
  COALESCE((SELECT MAX("LeadAssignmentId") FROM public."tblLeadAssignment"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblLeadConversion"', 'LeadConversionId'),
  COALESCE((SELECT MAX("LeadConversionId") FROM public."tblLeadConversion"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblLeadNote"', 'LeadNoteId'),
  COALESCE((SELECT MAX("LeadNoteId") FROM public."tblLeadNote"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblLeadStatusHistory"', 'LeadStatusHistoryId'),
  COALESCE((SELECT MAX("LeadStatusHistoryId") FROM public."tblLeadStatusHistory"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblQuotationHeader"', 'QuotationHeaderId'),
  COALESCE((SELECT MAX("QuotationHeaderId") FROM public."tblQuotationHeader"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblInvoiceHeader"', 'InvoiceHeaderId'),
  COALESCE((SELECT MAX("InvoiceHeaderId") FROM public."tblInvoiceHeader"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblCustomerAMC"', 'CustomerAmcId'),
  COALESCE((SELECT MAX("CustomerAmcId") FROM public."tblCustomerAMC"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblAMCVisitSchedule"', 'AmcVisitScheduleId'),
  COALESCE((SELECT MAX("AmcVisitScheduleId") FROM public."tblAMCVisitSchedule"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblPaymentTransaction"', 'PaymentTransactionId'),
  COALESCE((SELECT MAX("PaymentTransactionId") FROM public."tblPaymentTransaction"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblBillingStatusHistory"', 'BillingStatusHistoryId'),
  COALESCE((SELECT MAX("BillingStatusHistoryId") FROM public."tblBillingStatusHistory"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblPaymentReceipt"', 'PaymentReceiptId'),
  COALESCE((SELECT MAX("PaymentReceiptId") FROM public."tblPaymentReceipt"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblPaymentWebhookAttempt"', 'PaymentWebhookAttemptId'),
  COALESCE((SELECT MAX("PaymentWebhookAttemptId") FROM public."tblPaymentWebhookAttempt"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblQuotationLine"', 'QuotationLineId'),
  COALESCE((SELECT MAX("QuotationLineId") FROM public."tblQuotationLine"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblInvoiceLine"', 'InvoiceLineId'),
  COALESCE((SELECT MAX("InvoiceLineId") FROM public."tblInvoiceLine"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblRefundRequest"', 'RefundRequestId'),
  COALESCE((SELECT MAX("RefundRequestId") FROM public."tblRefundRequest"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblRefundApproval"', 'RefundApprovalId'),
  COALESCE((SELECT MAX("RefundApprovalId") FROM public."tblRefundApproval"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblRefundStatusHistory"', 'RefundStatusHistoryId'),
  COALESCE((SELECT MAX("RefundStatusHistoryId") FROM public."tblRefundStatusHistory"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblServiceRequestStatusHistory"', 'ServiceRequestStatusHistoryId'),
  COALESCE((SELECT MAX("ServiceRequestStatusHistoryId") FROM public."tblServiceRequestStatusHistory"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblTechnician"', 'TechnicianId'),
  COALESCE((SELECT MAX("TechnicianId") FROM public."tblTechnician"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblAssignmentLog"', 'AssignmentLogId'),
  COALESCE((SELECT MAX("AssignmentLogId") FROM public."tblAssignmentLog"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblCustomerAbsentRecord"', 'CustomerAbsentRecordId'),
  COALESCE((SELECT MAX("CustomerAbsentRecordId") FROM public."tblCustomerAbsentRecord"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblHelperAssignment"', 'HelperAssignmentId'),
  COALESCE((SELECT MAX("HelperAssignmentId") FROM public."tblHelperAssignment"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblHelperTaskResponse"', 'HelperTaskResponseId'),
  COALESCE((SELECT MAX("HelperTaskResponseId") FROM public."tblHelperTaskResponse"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblInstallationLead"', 'InstallationId'),
  COALESCE((SELECT MAX("InstallationId") FROM public."tblInstallationLead"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblInstallationChecklist"', 'InstallationChecklistId'),
  COALESCE((SELECT MAX("InstallationChecklistId") FROM public."tblInstallationChecklist"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblInstallationChecklistResponse"', 'InstallationChecklistResponseId'),
  COALESCE((SELECT MAX("InstallationChecklistResponseId") FROM public."tblInstallationChecklistResponse"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblInstallationProposal"', 'InstallationProposalId'),
  COALESCE((SELECT MAX("InstallationProposalId") FROM public."tblInstallationProposal"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblInstallationOrder"', 'InstallationOrderId'),
  COALESCE((SELECT MAX("InstallationOrderId") FROM public."tblInstallationOrder"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblCommissioningCertificate"', 'CommissioningCertificateId'),
  COALESCE((SELECT MAX("CommissioningCertificateId") FROM public."tblCommissioningCertificate"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblInstallationProposalLine"', 'InstallationProposalLineId'),
  COALESCE((SELECT MAX("InstallationProposalLineId") FROM public."tblInstallationProposalLine"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblInstallationStatusHistory"', 'InstallationStatusHistoryId'),
  COALESCE((SELECT MAX("InstallationStatusHistoryId") FROM public."tblInstallationStatusHistory"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblInstallationSurvey"', 'InstallationSurveyId'),
  COALESCE((SELECT MAX("InstallationSurveyId") FROM public."tblInstallationSurvey"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblInstallationSurveyItem"', 'InstallationSurveyItemId'),
  COALESCE((SELECT MAX("InstallationSurveyItemId") FROM public."tblInstallationSurveyItem"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblJobReport"', 'JobReportId'),
  COALESCE((SELECT MAX("JobReportId") FROM public."tblJobReport"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblCustomerSignature"', 'CustomerSignatureId'),
  COALESCE((SELECT MAX("CustomerSignatureId") FROM public."tblCustomerSignature"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblJobPhoto"', 'JobPhotoId'),
  COALESCE((SELECT MAX("JobPhotoId") FROM public."tblJobPhoto"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblPartsRequest"', 'PartsRequestId'),
  COALESCE((SELECT MAX("PartsRequestId") FROM public."tblPartsRequest"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblPartsRequestItem"', 'PartsRequestItemId'),
  COALESCE((SELECT MAX("PartsRequestItemId") FROM public."tblPartsRequestItem"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblPartsReturn"', 'PartsReturnId'),
  COALESCE((SELECT MAX("PartsReturnId") FROM public."tblPartsReturn"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblServiceRequestAssignment"', 'ServiceRequestAssignmentId'),
  COALESCE((SELECT MAX("ServiceRequestAssignmentId") FROM public."tblServiceRequestAssignment"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblSiteSurveyReport"', 'SiteSurveyReportId'),
  COALESCE((SELECT MAX("SiteSurveyReportId") FROM public."tblSiteSurveyReport"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblSkillAssessment"', 'SkillAssessmentId'),
  COALESCE((SELECT MAX("SkillAssessmentId") FROM public."tblSkillAssessment"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblStockTransaction"', 'StockTransactionId'),
  COALESCE((SELECT MAX("StockTransactionId") FROM public."tblStockTransaction"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblJobPartConsumption"', 'JobPartConsumptionId'),
  COALESCE((SELECT MAX("JobPartConsumptionId") FROM public."tblJobPartConsumption"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblTechnicianActivationLog"', 'TechnicianActivationLogId'),
  COALESCE((SELECT MAX("TechnicianActivationLogId") FROM public."tblTechnicianActivationLog"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblTechnicianAttendance"', 'TechnicianAttendanceId'),
  COALESCE((SELECT MAX("TechnicianAttendanceId") FROM public."tblTechnicianAttendance"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblTechnicianAvailability"', 'TechnicianAvailabilityId'),
  COALESCE((SELECT MAX("TechnicianAvailabilityId") FROM public."tblTechnicianAvailability"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblTechnicianDocument"', 'TechnicianDocumentId'),
  COALESCE((SELECT MAX("TechnicianDocumentId") FROM public."tblTechnicianDocument"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblTechnicianEarnings"', 'TechnicianEarningId'),
  COALESCE((SELECT MAX("TechnicianEarningId") FROM public."tblTechnicianEarnings"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblTechnicianGPSLog"', 'TechnicianGpsLogId'),
  COALESCE((SELECT MAX("TechnicianGpsLogId") FROM public."tblTechnicianGPSLog"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblTechnicianPerformanceSummary"', 'TechnicianPerformanceSummaryId'),
  COALESCE((SELECT MAX("TechnicianPerformanceSummaryId") FROM public."tblTechnicianPerformanceSummary"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblTechnicianSkill"', 'TechnicianSkillId'),
  COALESCE((SELECT MAX("TechnicianSkillId") FROM public."tblTechnicianSkill"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblTechnicianSkillMapping"', 'TechnicianSkillMappingId'),
  COALESCE((SELECT MAX("TechnicianSkillMappingId") FROM public."tblTechnicianSkillMapping"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblTechnicianVanStock"', 'TechnicianVanStockId'),
  COALESCE((SELECT MAX("TechnicianVanStockId") FROM public."tblTechnicianVanStock"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblTechnicianZone"', 'TechnicianZoneId'),
  COALESCE((SELECT MAX("TechnicianZoneId") FROM public."tblTechnicianZone"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblTrainingRecord"', 'TrainingRecordId'),
  COALESCE((SELECT MAX("TrainingRecordId") FROM public."tblTrainingRecord"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblWarrantyClaim"', 'WarrantyClaimId'),
  COALESCE((SELECT MAX("WarrantyClaimId") FROM public."tblWarrantyClaim"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblRevisitRequest"', 'RevisitRequestId'),
  COALESCE((SELECT MAX("RevisitRequestId") FROM public."tblRevisitRequest"), 1)
);

SELECT setval(
  pg_get_serial_sequence('public."tblZonePincode"', 'ZonePincodeId'),
  COALESCE((SELECT MAX("ZonePincodeId") FROM public."tblZonePincode"), 1)
);
