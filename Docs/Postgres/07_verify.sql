-- Coolzo PostgreSQL Migration | File 7 | Verification
-- Run last. Every table must appear. Row counts must match SQL Server.

SELECT 'tblAMCPlan' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblAMCPlan"
UNION ALL
SELECT 'tblAcType' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblAcType"
UNION ALL
SELECT 'tblAuditLog' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblAuditLog"
UNION ALL
SELECT 'tblBrand' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblBrand"
UNION ALL
SELECT 'tblBusinessHourConfiguration' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblBusinessHourConfiguration"
UNION ALL
SELECT 'tblCMSBanner' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblCMSBanner"
UNION ALL
SELECT 'tblCMSBlock' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblCMSBlock"
UNION ALL
SELECT 'tblCMSContentVersion' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblCMSContentVersion"
UNION ALL
SELECT 'tblCMSFaq' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblCMSFaq"
UNION ALL
SELECT 'tblCancellationPolicy' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblCancellationPolicy"
UNION ALL
SELECT 'tblDiagnosisResultMaster' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblDiagnosisResultMaster"
UNION ALL
SELECT 'tblDisplayContentSetting' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblDisplayContentSetting"
UNION ALL
SELECT 'tblDynamicMasterRecord' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblDynamicMasterRecord"
UNION ALL
SELECT 'tblFeatureFlag' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblFeatureFlag"
UNION ALL
SELECT 'tblHelperTaskChecklist' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblHelperTaskChecklist"
UNION ALL
SELECT 'tblHolidayConfiguration' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblHolidayConfiguration"
UNION ALL
SELECT 'tblItemCategory' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblItemCategory"
UNION ALL
SELECT 'tblLeadSource' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblLeadSource"
UNION ALL
SELECT 'tblNotificationTemplate' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblNotificationTemplate"
UNION ALL
SELECT 'tblNotificationTriggerConfiguration' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblNotificationTriggerConfiguration"
UNION ALL
SELECT 'tblOfflineSyncQueueItem' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblOfflineSyncQueueItem"
UNION ALL
SELECT 'tblPermission' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblPermission"
UNION ALL
SELECT 'tblPricingModel' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblPricingModel"
UNION ALL
SELECT 'tblRole' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblRole"
UNION ALL
SELECT 'tblRolePermission' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblRolePermission"
UNION ALL
SELECT 'tblServiceCategory' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblServiceCategory"
UNION ALL
SELECT 'tblService' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblService"
UNION ALL
SELECT 'tblComplaintIssueMaster' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblComplaintIssueMaster"
UNION ALL
SELECT 'tblServiceChecklistMaster' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblServiceChecklistMaster"
UNION ALL
SELECT 'tblSupplier' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblSupplier"
UNION ALL
SELECT 'tblPurchaseOrder' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblPurchaseOrder"
UNION ALL
SELECT 'tblSupportTicketCategory' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblSupportTicketCategory"
UNION ALL
SELECT 'tblSupportTicketPriority' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblSupportTicketPriority"
UNION ALL
SELECT 'tblSystemAlert' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblSystemAlert"
UNION ALL
SELECT 'tblSystemConfiguration' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblSystemConfiguration"
UNION ALL
SELECT 'tblSystemSetting' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblSystemSetting"
UNION ALL
SELECT 'tblTonnage' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblTonnage"
UNION ALL
SELECT 'tblUnitOfMeasure' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblUnitOfMeasure"
UNION ALL
SELECT 'tblItem' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblItem"
UNION ALL
SELECT 'tblItemRate' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblItemRate"
UNION ALL
SELECT 'tblPurchaseOrderItem' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblPurchaseOrderItem"
UNION ALL
SELECT 'tblUser' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblUser"
UNION ALL
SELECT 'tblCustomer' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblCustomer"
UNION ALL
SELECT 'tblCommunicationPreference' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblCommunicationPreference"
UNION ALL
SELECT 'tblCustomerEquipment' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblCustomerEquipment"
UNION ALL
SELECT 'tblHelperProfile' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblHelperProfile"
UNION ALL
SELECT 'tblHelperAttendance' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblHelperAttendance"
UNION ALL
SELECT 'tblOtpVerification' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblOtpVerification"
UNION ALL
SELECT 'tblSupportTicket' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblSupportTicket"
UNION ALL
SELECT 'tblSupportTicketAssignment' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblSupportTicketAssignment"
UNION ALL
SELECT 'tblSupportTicketEscalation' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblSupportTicketEscalation"
UNION ALL
SELECT 'tblSupportTicketLink' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblSupportTicketLink"
UNION ALL
SELECT 'tblSupportTicketReply' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblSupportTicketReply"
UNION ALL
SELECT 'tblSupportTicketStatusHistory' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblSupportTicketStatusHistory"
UNION ALL
SELECT 'tblUserPasswordHistory' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblUserPasswordHistory"
UNION ALL
SELECT 'tblUserRole' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblUserRole"
UNION ALL
SELECT 'tblUserSession' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblUserSession"
UNION ALL
SELECT 'tblRefreshToken' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblRefreshToken"
UNION ALL
SELECT 'tblWarehouse' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblWarehouse"
UNION ALL
SELECT 'tblWarehouseStock' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblWarehouseStock"
UNION ALL
SELECT 'tblWarrantyRule' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblWarrantyRule"
UNION ALL
SELECT 'tblWorkflowStatusHistory' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblWorkflowStatusHistory"
UNION ALL
SELECT 'tblZone' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblZone"
UNION ALL
SELECT 'tblCustomerAddress' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblCustomerAddress"
UNION ALL
SELECT 'tblSlotConfiguration' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblSlotConfiguration"
UNION ALL
SELECT 'tblSlotAvailability' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblSlotAvailability"
UNION ALL
SELECT 'tblBooking' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblBooking"
UNION ALL
SELECT 'tblBookingLine' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblBookingLine"
UNION ALL
SELECT 'tblBookingStatusHistory' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblBookingStatusHistory"
UNION ALL
SELECT 'tblCampaign' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblCampaign"
UNION ALL
SELECT 'tblCustomerReview' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblCustomerReview"
UNION ALL
SELECT 'tblServiceRequest' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblServiceRequest"
UNION ALL
SELECT 'tblCancellationRecord' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblCancellationRecord"
UNION ALL
SELECT 'tblJobCard' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblJobCard"
UNION ALL
SELECT 'tblJobAttachment' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblJobAttachment"
UNION ALL
SELECT 'tblJobChecklistResponse' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblJobChecklistResponse"
UNION ALL
SELECT 'tblJobDiagnosis' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblJobDiagnosis"
UNION ALL
SELECT 'tblJobExecutionNote' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblJobExecutionNote"
UNION ALL
SELECT 'tblJobExecutionTimeline' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblJobExecutionTimeline"
UNION ALL
SELECT 'tblLeads' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblLeads"
UNION ALL
SELECT 'tblLeadAssignment' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblLeadAssignment"
UNION ALL
SELECT 'tblLeadConversion' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblLeadConversion"
UNION ALL
SELECT 'tblLeadNote' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblLeadNote"
UNION ALL
SELECT 'tblLeadStatusHistory' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblLeadStatusHistory"
UNION ALL
SELECT 'tblQuotationHeader' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblQuotationHeader"
UNION ALL
SELECT 'tblInvoiceHeader' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblInvoiceHeader"
UNION ALL
SELECT 'tblCustomerAMC' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblCustomerAMC"
UNION ALL
SELECT 'tblAMCVisitSchedule' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblAMCVisitSchedule"
UNION ALL
SELECT 'tblPaymentTransaction' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblPaymentTransaction"
UNION ALL
SELECT 'tblBillingStatusHistory' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblBillingStatusHistory"
UNION ALL
SELECT 'tblPaymentReceipt' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblPaymentReceipt"
UNION ALL
SELECT 'tblPaymentWebhookAttempt' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblPaymentWebhookAttempt"
UNION ALL
SELECT 'tblQuotationLine' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblQuotationLine"
UNION ALL
SELECT 'tblInvoiceLine' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblInvoiceLine"
UNION ALL
SELECT 'tblRefundRequest' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblRefundRequest"
UNION ALL
SELECT 'tblRefundApproval' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblRefundApproval"
UNION ALL
SELECT 'tblRefundStatusHistory' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblRefundStatusHistory"
UNION ALL
SELECT 'tblServiceRequestStatusHistory' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblServiceRequestStatusHistory"
UNION ALL
SELECT 'tblTechnician' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblTechnician"
UNION ALL
SELECT 'tblAssignmentLog' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblAssignmentLog"
UNION ALL
SELECT 'tblCustomerAbsentRecord' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblCustomerAbsentRecord"
UNION ALL
SELECT 'tblHelperAssignment' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblHelperAssignment"
UNION ALL
SELECT 'tblHelperTaskResponse' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblHelperTaskResponse"
UNION ALL
SELECT 'tblInstallationLead' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblInstallationLead"
UNION ALL
SELECT 'tblInstallationChecklist' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblInstallationChecklist"
UNION ALL
SELECT 'tblInstallationChecklistResponse' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblInstallationChecklistResponse"
UNION ALL
SELECT 'tblInstallationProposal' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblInstallationProposal"
UNION ALL
SELECT 'tblInstallationOrder' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblInstallationOrder"
UNION ALL
SELECT 'tblCommissioningCertificate' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblCommissioningCertificate"
UNION ALL
SELECT 'tblInstallationProposalLine' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblInstallationProposalLine"
UNION ALL
SELECT 'tblInstallationStatusHistory' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblInstallationStatusHistory"
UNION ALL
SELECT 'tblInstallationSurvey' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblInstallationSurvey"
UNION ALL
SELECT 'tblInstallationSurveyItem' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblInstallationSurveyItem"
UNION ALL
SELECT 'tblJobReport' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblJobReport"
UNION ALL
SELECT 'tblCustomerSignature' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblCustomerSignature"
UNION ALL
SELECT 'tblJobPhoto' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblJobPhoto"
UNION ALL
SELECT 'tblPartsRequest' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblPartsRequest"
UNION ALL
SELECT 'tblPartsRequestItem' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblPartsRequestItem"
UNION ALL
SELECT 'tblPartsReturn' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblPartsReturn"
UNION ALL
SELECT 'tblServiceRequestAssignment' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblServiceRequestAssignment"
UNION ALL
SELECT 'tblSiteSurveyReport' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblSiteSurveyReport"
UNION ALL
SELECT 'tblSkillAssessment' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblSkillAssessment"
UNION ALL
SELECT 'tblStockTransaction' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblStockTransaction"
UNION ALL
SELECT 'tblJobPartConsumption' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblJobPartConsumption"
UNION ALL
SELECT 'tblTechnicianActivationLog' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblTechnicianActivationLog"
UNION ALL
SELECT 'tblTechnicianAttendance' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblTechnicianAttendance"
UNION ALL
SELECT 'tblTechnicianAvailability' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblTechnicianAvailability"
UNION ALL
SELECT 'tblTechnicianDocument' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblTechnicianDocument"
UNION ALL
SELECT 'tblTechnicianEarnings' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblTechnicianEarnings"
UNION ALL
SELECT 'tblTechnicianGPSLog' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblTechnicianGPSLog"
UNION ALL
SELECT 'tblTechnicianPerformanceSummary' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblTechnicianPerformanceSummary"
UNION ALL
SELECT 'tblTechnicianSkill' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblTechnicianSkill"
UNION ALL
SELECT 'tblTechnicianSkillMapping' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblTechnicianSkillMapping"
UNION ALL
SELECT 'tblTechnicianVanStock' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblTechnicianVanStock"
UNION ALL
SELECT 'tblTechnicianZone' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblTechnicianZone"
UNION ALL
SELECT 'tblTrainingRecord' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblTrainingRecord"
UNION ALL
SELECT 'tblWarrantyClaim' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblWarrantyClaim"
UNION ALL
SELECT 'tblRevisitRequest' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblRevisitRequest"
UNION ALL
SELECT 'tblZonePincode' AS "TableName", COUNT(1) AS "RowCount"
FROM public."tblZonePincode"
ORDER BY "TableName";

-- Expected row counts from SQL Server source:
-- tblAMCPlan: 3 rows
-- tblAcType: 3 rows
-- tblAuditLog: 473 rows
-- tblBrand: 3 rows
-- tblBusinessHourConfiguration: 7 rows
-- tblCMSBanner: 1 rows
-- tblCMSBlock: 3 rows
-- tblCMSContentVersion: 0 rows
-- tblCMSFaq: 2 rows
-- tblCancellationPolicy: 4 rows
-- tblDiagnosisResultMaster: 3 rows
-- tblDisplayContentSetting: 3 rows
-- tblDynamicMasterRecord: 19 rows
-- tblFeatureFlag: 8 rows
-- tblHelperTaskChecklist: 3 rows
-- tblHolidayConfiguration: 4 rows
-- tblItemCategory: 4 rows
-- tblLeadSource: 5 rows
-- tblNotificationTemplate: 8 rows
-- tblNotificationTriggerConfiguration: 8 rows
-- tblOfflineSyncQueueItem: 0 rows
-- tblPermission: 59 rows
-- tblPricingModel: 2 rows
-- tblRole: 12 rows
-- tblRolePermission: 237 rows
-- tblServiceCategory: 3 rows
-- tblService: 3 rows
-- tblComplaintIssueMaster: 3 rows
-- tblServiceChecklistMaster: 3 rows
-- tblSupplier: 4 rows
-- tblPurchaseOrder: 1 rows
-- tblSupportTicketCategory: 5 rows
-- tblSupportTicketPriority: 4 rows
-- tblSystemAlert: 238 rows
-- tblSystemConfiguration: 11 rows
-- tblSystemSetting: 26 rows
-- tblTonnage: 3 rows
-- tblUnitOfMeasure: 4 rows
-- tblItem: 4 rows
-- tblItemRate: 4 rows
-- tblPurchaseOrderItem: 1 rows
-- tblUser: 13 rows
-- tblCustomer: 2 rows
-- tblCommunicationPreference: 1 rows
-- tblCustomerEquipment: 0 rows
-- tblHelperProfile: 1 rows
-- tblHelperAttendance: 0 rows
-- tblOtpVerification: 138 rows
-- tblSupportTicket: 2 rows
-- tblSupportTicketAssignment: 2 rows
-- tblSupportTicketEscalation: 0 rows
-- tblSupportTicketLink: 2 rows
-- tblSupportTicketReply: 2 rows
-- tblSupportTicketStatusHistory: 2 rows
-- tblUserPasswordHistory: 2 rows
-- tblUserRole: 13 rows
-- tblUserSession: 212 rows
-- tblRefreshToken: 221 rows
-- tblWarehouse: 1 rows
-- tblWarehouseStock: 4 rows
-- tblWarrantyRule: 1 rows
-- tblWorkflowStatusHistory: 1 rows
-- tblZone: 2 rows
-- tblCustomerAddress: 3 rows
-- tblSlotConfiguration: 8 rows
-- tblSlotAvailability: 56 rows
-- tblBooking: 4 rows
-- tblBookingLine: 4 rows
-- tblBookingStatusHistory: 5 rows
-- tblCampaign: 0 rows
-- tblCustomerReview: 3 rows
-- tblServiceRequest: 4 rows
-- tblCancellationRecord: 1 rows
-- tblJobCard: 3 rows
-- tblJobAttachment: 0 rows
-- tblJobChecklistResponse: 0 rows
-- tblJobDiagnosis: 0 rows
-- tblJobExecutionNote: 1 rows
-- tblJobExecutionTimeline: 2 rows
-- tblLeads: 1 rows
-- tblLeadAssignment: 0 rows
-- tblLeadConversion: 0 rows
-- tblLeadNote: 0 rows
-- tblLeadStatusHistory: 1 rows
-- tblQuotationHeader: 2 rows
-- tblInvoiceHeader: 1 rows
-- tblCustomerAMC: 0 rows
-- tblAMCVisitSchedule: 0 rows
-- tblPaymentTransaction: 3 rows
-- tblBillingStatusHistory: 9 rows
-- tblPaymentReceipt: 3 rows
-- tblPaymentWebhookAttempt: 0 rows
-- tblQuotationLine: 4 rows
-- tblInvoiceLine: 2 rows
-- tblRefundRequest: 0 rows
-- tblRefundApproval: 0 rows
-- tblRefundStatusHistory: 0 rows
-- tblServiceRequestStatusHistory: 13 rows
-- tblTechnician: 2 rows
-- tblAssignmentLog: 1 rows
-- tblCustomerAbsentRecord: 0 rows
-- tblHelperAssignment: 0 rows
-- tblHelperTaskResponse: 0 rows
-- tblInstallationLead: 0 rows
-- tblInstallationChecklist: 0 rows
-- tblInstallationChecklistResponse: 0 rows
-- tblInstallationProposal: 0 rows
-- tblInstallationOrder: 0 rows
-- tblCommissioningCertificate: 0 rows
-- tblInstallationProposalLine: 0 rows
-- tblInstallationStatusHistory: 0 rows
-- tblInstallationSurvey: 0 rows
-- tblInstallationSurveyItem: 0 rows
-- tblJobReport: 0 rows
-- tblCustomerSignature: 0 rows
-- tblJobPhoto: 0 rows
-- tblPartsRequest: 2 rows
-- tblPartsRequestItem: 4 rows
-- tblPartsReturn: 0 rows
-- tblServiceRequestAssignment: 4 rows
-- tblSiteSurveyReport: 0 rows
-- tblSkillAssessment: 0 rows
-- tblStockTransaction: 7 rows
-- tblJobPartConsumption: 0 rows
-- tblTechnicianActivationLog: 0 rows
-- tblTechnicianAttendance: 3 rows
-- tblTechnicianAvailability: 14 rows
-- tblTechnicianDocument: 0 rows
-- tblTechnicianEarnings: 0 rows
-- tblTechnicianGPSLog: 0 rows
-- tblTechnicianPerformanceSummary: 4 rows
-- tblTechnicianSkill: 6 rows
-- tblTechnicianSkillMapping: 4 rows
-- tblTechnicianVanStock: 0 rows
-- tblTechnicianZone: 9 rows
-- tblTrainingRecord: 0 rows
-- tblWarrantyClaim: 0 rows
-- tblRevisitRequest: 0 rows
-- tblZonePincode: 4 rows
