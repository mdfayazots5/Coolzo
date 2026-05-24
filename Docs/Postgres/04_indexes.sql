-- Coolzo PostgreSQL Migration | File 4 | Indexes

CREATE INDEX "IDX_tblAMCPlan_IsActive_PriceAmount"
  ON public."tblAMCPlan" ("IsActive", "PriceAmount");

CREATE INDEX "IDX_tblAMCVisitSchedule_ScheduledDate_CurrentStatus"
  ON public."tblAMCVisitSchedule" ("ScheduledDate", "CurrentStatus");

CREATE INDEX "IDX_tblAssignmentLog_ServiceRequestId_ActionDateUtc"
  ON public."tblAssignmentLog" ("ServiceRequestId", "ActionDateUtc");

CREATE INDEX "IDX_tblAuditLog_UserId_TraceId"
  ON public."tblAuditLog" ("UserId", "TraceId");

CREATE INDEX "IDX_tblBillingStatusHistory_InvoiceHeaderId_StatusDateUtc"
  ON public."tblBillingStatusHistory" ("InvoiceHeaderId", "StatusDateUtc");

CREATE INDEX "IDX_tblBillingStatusHistory_PaymentTransactionId_StatusDateUtc"
  ON public."tblBillingStatusHistory" ("PaymentTransactionId", "StatusDateUtc");

CREATE INDEX "IDX_tblBillingStatusHistory_QuotationHeaderId_StatusDateUtc"
  ON public."tblBillingStatusHistory" ("QuotationHeaderId", "StatusDateUtc");

CREATE INDEX "IDX_tblBooking_BranchId_BookingDateUtc"
  ON public."tblBooking" ("BranchId", "BookingDateUtc");

CREATE INDEX "IDX_tblBooking_CustomerId_BookingDateUtc"
  ON public."tblBooking" ("CustomerId", "BookingDateUtc");

CREATE UNIQUE INDEX "UK_tblBooking_IdempotencyKey"
  ON public."tblBooking" ("IdempotencyKey");

CREATE INDEX "IDX_tblBookingStatusHistory_BookingId_StatusDateUtc"
  ON public."tblBookingStatusHistory" ("BookingId", "StatusDateUtc");

CREATE INDEX "IDX_tblCancellationPolicy_BranchId_PolicyCode_CustomerTypeCode"
  ON public."tblCancellationPolicy" ("BranchId", "PolicyCode", "CustomerTypeCode");

CREATE INDEX "IDX_tblCancellationRecord_BookingId"
  ON public."tblCancellationRecord" ("BookingId");

CREATE INDEX "IDX_tblCancellationRecord_BranchId_Status_DateCreated"
  ON public."tblCancellationRecord" ("BranchId", "CancellationStatus", "DateCreated");

CREATE UNIQUE INDEX "UK_tblCancellationRecord_ServiceRequestId"
  ON public."tblCancellationRecord" ("ServiceRequestId");

CREATE INDEX "IDX_tblCMSBanner_DisplayArea_SortOrder"
  ON public."tblCMSBanner" ("DisplayArea", "SortOrder");

CREATE INDEX "IDX_tblCMSFaq_Category_SortOrder"
  ON public."tblCMSFaq" ("Category", "SortOrder");

CREATE UNIQUE INDEX "UK_tblCommissioningCertificate_WarrantyRegistrationNumber"
  ON public."tblCommissioningCertificate" ("WarrantyRegistrationNumber");

CREATE INDEX "IDX_tblComplaintIssueMaster_ServiceId_IssueName"
  ON public."tblComplaintIssueMaster" ("ServiceId", "IssueName");

CREATE INDEX "IDX_tblCustomer_MobileNumber"
  ON public."tblCustomer" ("MobileNumber");

CREATE UNIQUE INDEX "UK_tblCustomer_UserId"
  ON public."tblCustomer" ("UserId");

CREATE INDEX "IDX_tblCustomerAbsentRecord_BranchId_Status_MarkedOn"
  ON public."tblCustomerAbsentRecord" ("BranchId", "CustomerAbsentStatus", "MarkedOn");

CREATE UNIQUE INDEX "UK_tblCustomerAbsentRecord_ServiceRequestId"
  ON public."tblCustomerAbsentRecord" ("ServiceRequestId");

CREATE INDEX "IDX_tblCustomerAddress_CustomerId_Pincode"
  ON public."tblCustomerAddress" ("CustomerId", "Pincode");

CREATE INDEX "IDX_tblCustomerAMC_CustomerId_CurrentStatus_EndDateUtc"
  ON public."tblCustomerAMC" ("CustomerId", "CurrentStatus", "EndDateUtc");

CREATE INDEX "IDX_tblCustomerEquipment_CustomerId_IsActive"
  ON public."tblCustomerEquipment" ("CustomerId", "IsActive");

CREATE INDEX "IDX_tblCustomerReview_ServiceId_IsActive_DateCreated"
  ON public."tblCustomerReview" ("ServiceId", "IsActive", "DateCreated");

CREATE INDEX "IDX_tblDynamicMasterRecord_MasterType_SortOrder"
  ON public."tblDynamicMasterRecord" ("MasterType", "SortOrder");

CREATE INDEX "IDX_tblHelperAssignment_HelperProfileId_AssignmentStatus"
  ON public."tblHelperAssignment" ("HelperProfileId", "AssignmentStatus");

CREATE INDEX "IDX_tblHelperAttendance_HelperProfileId_AttendanceDate"
  ON public."tblHelperAttendance" ("HelperProfileId", "AttendanceDate");

CREATE UNIQUE INDEX "UK_tblHelperProfile_HelperCode"
  ON public."tblHelperProfile" ("HelperCode");

CREATE UNIQUE INDEX "UK_tblHelperProfile_UserId"
  ON public."tblHelperProfile" ("UserId");

CREATE INDEX "IDX_tblHelperTaskChecklist_ServiceTypeId_SortOrder"
  ON public."tblHelperTaskChecklist" ("ServiceTypeId", "SortOrder");

CREATE UNIQUE INDEX "UK_tblHelperTaskResponse_HelperAssignment_Checklist"
  ON public."tblHelperTaskResponse" ("HelperAssignmentId", "HelperTaskChecklistId");

CREATE INDEX "IDX_tblInstallationChecklist_InstallationId"
  ON public."tblInstallationChecklist" ("InstallationId");

CREATE INDEX "IDX_tblInstallationChecklistResponse_InstallationChecklistId_InstallationId"
  ON public."tblInstallationChecklistResponse" ("InstallationChecklistId", "InstallationId");

CREATE INDEX "IDX_tblInstallationLead_CustomerId_InstallationStatus"
  ON public."tblInstallationLead" ("CustomerId", "InstallationStatus");

CREATE UNIQUE INDEX "UK_tblInstallationLead_InstallationNumber"
  ON public."tblInstallationLead" ("InstallationNumber");

CREATE INDEX "IDX_tblInstallationOrder_CurrentStatus_ScheduledInstallationDateUtc"
  ON public."tblInstallationOrder" ("CurrentStatus", "ScheduledInstallationDateUtc");

CREATE INDEX "IDX_tblInstallationOrder_InstallationId"
  ON public."tblInstallationOrder" ("InstallationId");

CREATE INDEX "IDX_tblInstallationOrder_InstallationProposalId"
  ON public."tblInstallationOrder" ("InstallationProposalId");

CREATE INDEX "IDX_tblInstallationOrder_TechnicianId"
  ON public."tblInstallationOrder" ("TechnicianId");

CREATE INDEX "IDX_tblInstallationProposal_InstallationId_ProposalStatus"
  ON public."tblInstallationProposal" ("InstallationId", "ProposalStatus");

CREATE UNIQUE INDEX "UK_tblInstallationProposal_ProposalNumber"
  ON public."tblInstallationProposal" ("ProposalNumber");

CREATE INDEX "IDX_tblInstallationProposalLine_InstallationProposalId"
  ON public."tblInstallationProposalLine" ("InstallationProposalId");

CREATE INDEX "IDX_tblInstallationStatusHistory_InstallationId_ChangedDateUtc"
  ON public."tblInstallationStatusHistory" ("InstallationId", "ChangedDateUtc");

CREATE INDEX "IDX_tblInstallationSurvey_InstallationId_SurveyDateUtc"
  ON public."tblInstallationSurvey" ("InstallationId", "SurveyDateUtc");

CREATE INDEX "IDX_tblInstallationSurveyItem_InstallationSurveyId"
  ON public."tblInstallationSurveyItem" ("InstallationSurveyId");

CREATE INDEX "IDX_tblInvoiceHeader_CustomerId_CurrentStatus_BranchId"
  ON public."tblInvoiceHeader" ("CustomerId", "CurrentStatus", "BranchId");

CREATE INDEX "IDX_tblInvoiceHeader_CustomerId_InvoiceDateUtc"
  ON public."tblInvoiceHeader" ("CustomerId", "InvoiceDateUtc");

CREATE INDEX "IDX_tblInvoiceLine_InvoiceHeaderId"
  ON public."tblInvoiceLine" ("InvoiceHeaderId");

CREATE INDEX "IDX_tblItem_IsActive_ItemName"
  ON public."tblItem" ("IsActive", "ItemName");

CREATE INDEX "IDX_tblItemCategory_IsActive_CategoryName"
  ON public."tblItemCategory" ("IsActive", "CategoryName");

CREATE INDEX "IDX_tblItemRate_ItemId_EffectiveFromUtc"
  ON public."tblItemRate" ("ItemId", "EffectiveFromUtc");

CREATE INDEX "IDX_tblJobAttachment_JobCardId_AttachmentType"
  ON public."tblJobAttachment" ("JobCardId", "AttachmentType");

CREATE INDEX "IDX_tblJobExecutionNote_JobCardId_NoteDateUtc"
  ON public."tblJobExecutionNote" ("JobCardId", "NoteDateUtc");

CREATE INDEX "IDX_tblJobExecutionTimeline_JobCardId_EventDateUtc"
  ON public."tblJobExecutionTimeline" ("JobCardId", "EventDateUtc");

CREATE INDEX "IDX_tblJobPartConsumption_JobCardId_ConsumedDateUtc"
  ON public."tblJobPartConsumption" ("JobCardId", "ConsumedDateUtc");

CREATE INDEX "IDX_tblJobReport_IdempotencyKey"
  ON public."tblJobReport" ("IdempotencyKey");

CREATE INDEX "IDX_tblJobReport_ServiceRequestId_SubmittedAtUtc"
  ON public."tblJobReport" ("ServiceRequestId", "SubmittedAtUtc");

CREATE INDEX "IDX_tblLeadAssignment_AssignedUserId_AssignedDateUtc"
  ON public."tblLeadAssignment" ("AssignedUserId", "AssignedDateUtc");

CREATE INDEX "IDX_tblLeadAssignment_LeadId_AssignedDateUtc"
  ON public."tblLeadAssignment" ("LeadId", "AssignedDateUtc");

CREATE INDEX "IDX_tblLeadConversion_BookingId_ServiceRequestId"
  ON public."tblLeadConversion" ("BookingId", "ServiceRequestId");

CREATE INDEX "IDX_tblLeadNote_LeadId_NoteDateUtc"
  ON public."tblLeadNote" ("LeadId", "NoteDateUtc");

CREATE INDEX "IDX_tblLeads_BranchId_LeadStatus_DateCreated"
  ON public."tblLeads" ("BranchId", "LeadStatus", "DateCreated");

CREATE INDEX "IDX_tblLeads_LeadStatus_AssignedUserId"
  ON public."tblLeads" ("LeadStatus", "AssignedUserId");

CREATE INDEX "IDX_tblLeads_MobileNumber_DateCreated"
  ON public."tblLeads" ("MobileNumber", "DateCreated");

CREATE INDEX "IDX_tblLeads_SourceChannel_DateCreated"
  ON public."tblLeads" ("SourceChannel", "DateCreated");

CREATE INDEX "IDX_tblLeadSource_BranchId_IsActive"
  ON public."tblLeadSource" ("BranchId", "IsActive");

CREATE INDEX "IDX_tblLeadStatusHistory_BranchId_LeadId_ChangedDateUtc"
  ON public."tblLeadStatusHistory" ("BranchId", "LeadId", "ChangedDateUtc");

CREATE INDEX "IDX_tblLeadStatusHistory_LeadId_ChangedDateUtc"
  ON public."tblLeadStatusHistory" ("LeadId", "ChangedDateUtc");

CREATE INDEX "IDX_tblNotificationTemplate_TriggerCode_Channel"
  ON public."tblNotificationTemplate" ("TriggerCode", "Channel");

CREATE INDEX "IDX_tblOfflineSyncQueueItem_EntityName_EntityReference"
  ON public."tblOfflineSyncQueueItem" ("EntityName", "EntityReference");

CREATE INDEX "IDX_tblOfflineSyncQueueItem_SyncStatus_NextRetryDateUtc"
  ON public."tblOfflineSyncQueueItem" ("SyncStatus", "NextRetryDateUtc");

CREATE INDEX "IDX_tblOtpVerification_UserId"
  ON public."tblOtpVerification" ("UserId");

CREATE INDEX "IDX_tblPartsRequest_CurrentStatus_Urgency"
  ON public."tblPartsRequest" ("CurrentStatus", "Urgency");

CREATE INDEX "IDX_tblPartsRequest_ServiceRequestId_SubmittedAtUtc"
  ON public."tblPartsRequest" ("ServiceRequestId", "SubmittedAtUtc");

CREATE INDEX "IDX_tblPartsRequestItem_ItemId"
  ON public."tblPartsRequestItem" ("ItemId");

CREATE INDEX "IDX_tblPartsRequestItem_PartsRequestId"
  ON public."tblPartsRequestItem" ("PartsRequestId");

CREATE INDEX "IDX_tblPaymentTransaction_GatewayTransactionId"
  ON public."tblPaymentTransaction" ("GatewayTransactionId");

CREATE INDEX "IDX_tblPaymentTransaction_IdempotencyKey"
  ON public."tblPaymentTransaction" ("IdempotencyKey");

CREATE INDEX "IDX_tblPaymentTransaction_InvoiceHeaderId_PaymentDateUtc"
  ON public."tblPaymentTransaction" ("InvoiceHeaderId", "PaymentDateUtc");

CREATE INDEX "IDX_tblPaymentWebhookAttempt_AttemptStatus_NextRetryDateUtc"
  ON public."tblPaymentWebhookAttempt" ("AttemptStatus", "NextRetryDateUtc");

CREATE INDEX "IDX_tblPaymentWebhookAttempt_IdempotencyKey"
  ON public."tblPaymentWebhookAttempt" ("IdempotencyKey");

CREATE INDEX "IDX_tblPermission_ModuleName_ActionName"
  ON public."tblPermission" ("ModuleName", "ActionName");

CREATE INDEX "IDX_tblPurchaseOrder_CurrentStatus_ExpectedDeliveryDateUtc"
  ON public."tblPurchaseOrder" ("CurrentStatus", "ExpectedDeliveryDateUtc");

CREATE UNIQUE INDEX "UK_tblPurchaseOrder_PONumber"
  ON public."tblPurchaseOrder" ("PONumber");

CREATE INDEX "IDX_tblPurchaseOrderItem_ItemId"
  ON public."tblPurchaseOrderItem" ("ItemId");

CREATE INDEX "IDX_tblPurchaseOrderItem_PurchaseOrderId"
  ON public."tblPurchaseOrderItem" ("PurchaseOrderId");

CREATE INDEX "IDX_tblQuotationHeader_CustomerId_QuotationDateUtc"
  ON public."tblQuotationHeader" ("CustomerId", "QuotationDateUtc");

CREATE INDEX "IDX_tblQuotationLine_QuotationHeaderId"
  ON public."tblQuotationLine" ("QuotationHeaderId");

CREATE INDEX "IDX_tblRefreshToken_UserId"
  ON public."tblRefreshToken" ("UserId");

CREATE INDEX "IDX_tblRefreshToken_UserSessionId"
  ON public."tblRefreshToken" ("UserSessionId");

CREATE INDEX "IDX_tblRefundApproval_RefundRequestId_ApprovalLevel"
  ON public."tblRefundApproval" ("RefundRequestId", "ApprovalLevel");

CREATE INDEX "IDX_tblRefundRequest_BranchId_Status_DateCreated"
  ON public."tblRefundRequest" ("BranchId", "RefundStatus", "DateCreated");

CREATE INDEX "IDX_tblRefundRequest_InvoiceHeaderId_RequestedDateUtc"
  ON public."tblRefundRequest" ("InvoiceHeaderId", "RequestedDateUtc");

CREATE UNIQUE INDEX "UK_tblRefundRequest_RefundRequestNo"
  ON public."tblRefundRequest" ("RefundRequestNo");

CREATE INDEX "IDX_tblRefundStatusHistory_RefundRequestId_ChangedOn"
  ON public."tblRefundStatusHistory" ("RefundRequestId", "ChangedOn");

CREATE INDEX "IDX_tblRevisitRequest_BookingId_RequestedDateUtc"
  ON public."tblRevisitRequest" ("BookingId", "RequestedDateUtc");

CREATE UNIQUE INDEX "UK_tblRevisitRequest_WarrantyClaimId"
  ON public."tblRevisitRequest" ("WarrantyClaimId");

CREATE INDEX "IDX_tblRole_CompanyId_SiteId"
  ON public."tblRole" ("CompanyId", "SiteId");

CREATE INDEX "IDX_tblRolePermission_PermissionId"
  ON public."tblRolePermission" ("PermissionId");

CREATE INDEX "IDX_tblRolePermission_RoleId"
  ON public."tblRolePermission" ("RoleId");

CREATE INDEX "IDX_tblService_ServiceCategoryId_ServiceName"
  ON public."tblService" ("ServiceCategoryId", "ServiceName");

CREATE INDEX "IDX_tblServiceCategory_CategoryName"
  ON public."tblServiceCategory" ("CategoryName");

CREATE INDEX "IDX_tblServiceRequest_CurrentStatus_BranchId_ServiceRequestDateUtc"
  ON public."tblServiceRequest" ("CurrentStatus", "BranchId", "ServiceRequestDateUtc");

CREATE INDEX "IDX_tblServiceRequestAssignment_ServiceRequestId_IsActiveAssignment"
  ON public."tblServiceRequestAssignment" ("ServiceRequestId", "IsActiveAssignment");

CREATE INDEX "IDX_tblServiceRequestAssignment_TechnicianId_AssignedDateUtc"
  ON public."tblServiceRequestAssignment" ("TechnicianId", "AssignedDateUtc");

CREATE INDEX "IDX_tblServiceRequestStatusHistory_ServiceRequestId_StatusDateUtc"
  ON public."tblServiceRequestStatusHistory" ("ServiceRequestId", "StatusDateUtc");

CREATE INDEX "IDX_tblSkillAssessment_TechnicianId_AssessedOnUtc"
  ON public."tblSkillAssessment" ("TechnicianId", "AssessedOnUtc");

CREATE INDEX "IDX_tblStockTransaction_ItemId_TransactionDateUtc"
  ON public."tblStockTransaction" ("ItemId", "TransactionDateUtc");

CREATE INDEX "IDX_tblStockTransaction_TechnicianId_TransactionDateUtc"
  ON public."tblStockTransaction" ("TechnicianId", "TransactionDateUtc");

CREATE INDEX "IDX_tblStockTransaction_TransactionGroupCode"
  ON public."tblStockTransaction" ("TransactionGroupCode");

CREATE INDEX "IDX_tblStockTransaction_WarehouseId_TransactionDateUtc"
  ON public."tblStockTransaction" ("WarehouseId", "TransactionDateUtc");

CREATE INDEX "IDX_tblSupplier_IsActive_SupplierName"
  ON public."tblSupplier" ("IsActive", "SupplierName");

CREATE INDEX "IDX_tblSupportTicket_CurrentStatus_BranchId_DateCreated"
  ON public."tblSupportTicket" ("CurrentStatus", "BranchId", "DateCreated");

CREATE INDEX "IDX_tblSupportTicket_CurrentStatus_SupportTicketPriorityId"
  ON public."tblSupportTicket" ("CurrentStatus", "SupportTicketPriorityId", "LastUpdated", "DateCreated");

CREATE INDEX "IDX_tblSupportTicket_CustomerId_DateCreated"
  ON public."tblSupportTicket" ("CustomerId", "DateCreated");

CREATE INDEX "IDX_tblSupportTicketAssignment_AssignedUserId_IsActiveAssignment"
  ON public."tblSupportTicketAssignment" ("AssignedUserId", "IsActiveAssignment", "AssignedDateUtc");

CREATE INDEX "IDX_tblSupportTicketAssignment_SupportTicketId_IsActiveAssignment"
  ON public."tblSupportTicketAssignment" ("SupportTicketId", "IsActiveAssignment", "AssignedDateUtc");

CREATE INDEX "IDX_tblSupportTicketCategory_IsActive_SortOrder"
  ON public."tblSupportTicketCategory" ("IsActive", "SortOrder", "CategoryName");

CREATE INDEX "IDX_tblSupportTicketEscalation_SupportTicketId_EscalatedDateUtc"
  ON public."tblSupportTicketEscalation" ("SupportTicketId", "EscalatedDateUtc");

CREATE INDEX "IDX_tblSupportTicketLink_SupportTicketId_LinkedEntityType_LinkedEntityId"
  ON public."tblSupportTicketLink" ("SupportTicketId", "LinkedEntityType", "LinkedEntityId");

CREATE INDEX "IDX_tblSupportTicketPriority_PriorityRank_SortOrder"
  ON public."tblSupportTicketPriority" ("PriorityRank", "SortOrder", "PriorityName");

CREATE INDEX "IDX_tblSupportTicketReply_SupportTicketId_ReplyDateUtc"
  ON public."tblSupportTicketReply" ("SupportTicketId", "ReplyDateUtc");

CREATE INDEX "IDX_tblSupportTicketStatusHistory_SupportTicketId_StatusDateUtc"
  ON public."tblSupportTicketStatusHistory" ("SupportTicketId", "StatusDateUtc");

CREATE INDEX "IDX_tblSystemAlert_AlertStatus_SlaDueDateUtc"
  ON public."tblSystemAlert" ("AlertStatus", "SlaDueDateUtc");

CREATE INDEX "IDX_tblTechnician_IsActive_BaseZoneId"
  ON public."tblTechnician" ("IsActive", "BaseZoneId");

CREATE INDEX "IDX_tblTechnicianActivationLog_TechnicianId_ActivatedOnUtc"
  ON public."tblTechnicianActivationLog" ("TechnicianId", "ActivatedOnUtc");

CREATE INDEX "IDX_tblTechnicianDocument_TechnicianId_DocumentType"
  ON public."tblTechnicianDocument" ("TechnicianId", "DocumentType");

CREATE INDEX "IDX_tblTechnicianEarnings_TechnicianId_CalculatedDateUtc"
  ON public."tblTechnicianEarnings" ("TechnicianId", "CalculatedDateUtc");

CREATE INDEX "IDX_tblTechnicianGPSLog_TechnicianId_TrackedOnUtc"
  ON public."tblTechnicianGPSLog" ("TechnicianId", "TrackedOnUtc");

CREATE UNIQUE INDEX "UK_tblTechnicianSkill_TechnicianId_SkillName"
  ON public."tblTechnicianSkill" ("TechnicianId", "SkillName");

CREATE INDEX "IDX_tblTechnicianSkillMapping_TechnicianId_ServiceId_AcTypeId"
  ON public."tblTechnicianSkillMapping" ("TechnicianId", "ServiceId", "AcTypeId");

CREATE INDEX "IDX_tblTechnicianVanStock_TechnicianId_LastTransactionDateUtc"
  ON public."tblTechnicianVanStock" ("TechnicianId", "LastTransactionDateUtc");

CREATE UNIQUE INDEX "UK_tblTechnicianZone_TechnicianId_ZoneId"
  ON public."tblTechnicianZone" ("TechnicianId", "ZoneId");

CREATE INDEX "IDX_tblTrainingRecord_TechnicianId_CompletionDateUtc"
  ON public."tblTrainingRecord" ("TechnicianId", "CompletionDateUtc");

CREATE INDEX "IDX_tblUnitOfMeasure_IsActive_UnitName"
  ON public."tblUnitOfMeasure" ("IsActive", "UnitName");

CREATE INDEX "IDX_tblUser_CompanyId_SiteId"
  ON public."tblUser" ("CompanyId", "SiteId");

CREATE INDEX "IDX_tblUserPasswordHistory_UserId_ChangedOnUtc"
  ON public."tblUserPasswordHistory" ("UserId", "ChangedOnUtc");

CREATE INDEX "IDX_tblUserRole_RoleId"
  ON public."tblUserRole" ("RoleId");

CREATE INDEX "IDX_tblUserRole_UserId"
  ON public."tblUserRole" ("UserId");

CREATE INDEX "IDX_tblUserSession_UserId"
  ON public."tblUserSession" ("UserId");

CREATE INDEX "IDX_tblWarehouse_IsActive_WarehouseName"
  ON public."tblWarehouse" ("IsActive", "WarehouseName");

CREATE INDEX "IDX_tblWarehouseStock_WarehouseId_LastTransactionDateUtc"
  ON public."tblWarehouseStock" ("WarehouseId", "LastTransactionDateUtc");

CREATE INDEX "IDX_tblWarrantyClaim_InvoiceHeaderId_CurrentStatus_ClaimDateUtc"
  ON public."tblWarrantyClaim" ("InvoiceHeaderId", "CurrentStatus", "ClaimDateUtc");

CREATE INDEX "IDX_tblWarrantyRule_ServiceId_AcTypeId_BrandId"
  ON public."tblWarrantyRule" ("ServiceId", "AcTypeId", "BrandId", "IsActive");

CREATE INDEX "IDX_tblWorkflowStatusHistory_EntityType_EntityReference_ChangedDateUtc"
  ON public."tblWorkflowStatusHistory" ("EntityType", "EntityReference", "ChangedDateUtc");

CREATE INDEX "IDX_tblZonePincode_Pincode"
  ON public."tblZonePincode" ("Pincode");

CREATE UNIQUE INDEX "UK_tblAcType_AcTypeCode"
  ON public."tblAcType" ("AcTypeCode");

CREATE UNIQUE INDEX "UK_tblAMCPlan_PlanName"
  ON public."tblAMCPlan" ("PlanName");

CREATE UNIQUE INDEX "UK_tblAMCVisitSchedule_CustomerAmcId_VisitNumber"
  ON public."tblAMCVisitSchedule" ("CustomerAmcId", "VisitNumber");

CREATE UNIQUE INDEX "UK_tblBooking_BookingReference"
  ON public."tblBooking" ("BookingReference");

CREATE UNIQUE INDEX "UK_tblBrand_BrandCode"
  ON public."tblBrand" ("BrandCode");

CREATE UNIQUE INDEX "UK_tblBusinessHourConfiguration_DayOfWeekNumber"
  ON public."tblBusinessHourConfiguration" ("DayOfWeekNumber");

CREATE UNIQUE INDEX "UK_tblCampaign_CampaignCode"
  ON public."tblCampaign" ("CampaignCode");

CREATE UNIQUE INDEX "UK_tblCMSBlock_BlockKey"
  ON public."tblCMSBlock" ("BlockKey");

CREATE UNIQUE INDEX "UK_tblCMSContentVersion_ContentType_ContentId_VersionNumber"
  ON public."tblCMSContentVersion" ("ContentType", "ContentId", "VersionNumber");

CREATE UNIQUE INDEX "UK_tblCommissioningCertificate_CertificateNumber"
  ON public."tblCommissioningCertificate" ("CertificateNumber");

CREATE UNIQUE INDEX "UK_tblCommunicationPreference_CustomerId"
  ON public."tblCommunicationPreference" ("CustomerId");

CREATE UNIQUE INDEX "UK_tblCustomerAMC_InvoiceHeaderId"
  ON public."tblCustomerAMC" ("InvoiceHeaderId");

CREATE UNIQUE INDEX "UK_tblDiagnosisResultMaster_ResultName"
  ON public."tblDiagnosisResultMaster" ("ResultName");

CREATE UNIQUE INDEX "UK_tblDisplayContentSetting_ContentGroup_ContentKey"
  ON public."tblDisplayContentSetting" ("ContentGroup", "ContentKey");

CREATE UNIQUE INDEX "UK_tblDynamicMasterRecord_MasterType_MasterCode"
  ON public."tblDynamicMasterRecord" ("MasterType", "MasterCode");

CREATE UNIQUE INDEX "UK_tblFeatureFlag_FlagCode"
  ON public."tblFeatureFlag" ("FlagCode");

CREATE UNIQUE INDEX "UK_tblHolidayConfiguration_HolidayDate"
  ON public."tblHolidayConfiguration" ("HolidayDate");

CREATE UNIQUE INDEX "UK_tblInstallationOrder_InstallationOrderNumber"
  ON public."tblInstallationOrder" ("InstallationOrderNumber");

CREATE UNIQUE INDEX "UK_tblInvoiceHeader_InvoiceNumber"
  ON public."tblInvoiceHeader" ("InvoiceNumber");

CREATE UNIQUE INDEX "UK_tblInvoiceHeader_QuotationHeaderId"
  ON public."tblInvoiceHeader" ("QuotationHeaderId");

CREATE UNIQUE INDEX "UK_tblItem_ItemCode"
  ON public."tblItem" ("ItemCode");

CREATE UNIQUE INDEX "UK_tblItemCategory_CategoryCode"
  ON public."tblItemCategory" ("CategoryCode");

CREATE UNIQUE INDEX "UK_tblJobCard_JobCardNumber"
  ON public."tblJobCard" ("JobCardNumber");

CREATE UNIQUE INDEX "UK_tblJobCard_ServiceRequestId"
  ON public."tblJobCard" ("ServiceRequestId");

CREATE UNIQUE INDEX "UK_tblJobChecklistResponse_JobCardId_ServiceChecklistMasterId"
  ON public."tblJobChecklistResponse" ("JobCardId", "ServiceChecklistMasterId");

CREATE UNIQUE INDEX "UK_tblJobDiagnosis_JobCardId"
  ON public."tblJobDiagnosis" ("JobCardId");

CREATE UNIQUE INDEX "UK_tblJobPartConsumption_StockTransactionId"
  ON public."tblJobPartConsumption" ("StockTransactionId");

CREATE UNIQUE INDEX "UK_tblLeadConversion_LeadId_ConversionType"
  ON public."tblLeadConversion" ("LeadId", "ConversionType");

CREATE UNIQUE INDEX "UK_tblLeads_LeadNumber"
  ON public."tblLeads" ("LeadNumber");

CREATE UNIQUE INDEX "UK_tblLeadSource_SourceCode"
  ON public."tblLeadSource" ("SourceCode");

CREATE UNIQUE INDEX "UK_tblNotificationTemplate_TemplateCode"
  ON public."tblNotificationTemplate" ("TemplateCode");

CREATE UNIQUE INDEX "UK_tblNotificationTriggerConfiguration_TriggerCode"
  ON public."tblNotificationTriggerConfiguration" ("TriggerCode");

CREATE UNIQUE INDEX "UK_tblPartsReturn_PartsReturnNumber"
  ON public."tblPartsReturn" ("PartsReturnNumber");

CREATE UNIQUE INDEX "UK_tblPaymentReceipt_PaymentTransactionId"
  ON public."tblPaymentReceipt" ("PaymentTransactionId");

CREATE UNIQUE INDEX "UK_tblPaymentReceipt_ReceiptNumber"
  ON public."tblPaymentReceipt" ("ReceiptNumber");

CREATE UNIQUE INDEX "UK_tblPermission_PermissionName"
  ON public."tblPermission" ("PermissionName");

CREATE UNIQUE INDEX "UK_tblPricingModel_PricingModelName"
  ON public."tblPricingModel" ("PricingModelName");

CREATE UNIQUE INDEX "UK_tblQuotationHeader_JobCardId"
  ON public."tblQuotationHeader" ("JobCardId");

CREATE UNIQUE INDEX "UK_tblQuotationHeader_QuotationNumber"
  ON public."tblQuotationHeader" ("QuotationNumber");

CREATE UNIQUE INDEX "UK_tblRefreshToken_TokenValue"
  ON public."tblRefreshToken" ("TokenValue");

CREATE UNIQUE INDEX "UK_tblRole_RoleName"
  ON public."tblRole" ("RoleName");

CREATE UNIQUE INDEX "UK_tblRolePermission_RoleId_PermissionId"
  ON public."tblRolePermission" ("RoleId", "PermissionId");

CREATE UNIQUE INDEX "UK_tblService_ServiceCode"
  ON public."tblService" ("ServiceCode");

CREATE UNIQUE INDEX "UK_tblServiceCategory_CategoryCode"
  ON public."tblServiceCategory" ("CategoryCode");

CREATE UNIQUE INDEX "UK_tblServiceChecklistMaster_ServiceId_ChecklistTitle"
  ON public."tblServiceChecklistMaster" ("ServiceId", "ChecklistTitle");

CREATE UNIQUE INDEX "UK_tblServiceRequest_BookingId"
  ON public."tblServiceRequest" ("BookingId");

CREATE UNIQUE INDEX "UK_tblServiceRequest_ServiceRequestNumber"
  ON public."tblServiceRequest" ("ServiceRequestNumber");

CREATE UNIQUE INDEX "UK_tblSlotAvailability_ZoneId_SlotDate_SlotConfigurationId"
  ON public."tblSlotAvailability" ("ZoneId", "SlotDate", "SlotConfigurationId");

CREATE UNIQUE INDEX "UK_tblSlotConfiguration_ZoneId_StartTime_EndTime"
  ON public."tblSlotConfiguration" ("ZoneId", "StartTime", "EndTime");

CREATE UNIQUE INDEX "UK_tblSupplier_SupplierCode"
  ON public."tblSupplier" ("SupplierCode");

CREATE UNIQUE INDEX "UK_tblSupportTicket_TicketNumber"
  ON public."tblSupportTicket" ("TicketNumber");

CREATE UNIQUE INDEX "UK_tblSupportTicketCategory_CategoryCode"
  ON public."tblSupportTicketCategory" ("CategoryCode");

CREATE UNIQUE INDEX "UK_tblSupportTicketPriority_PriorityCode"
  ON public."tblSupportTicketPriority" ("PriorityCode");

CREATE UNIQUE INDEX "UK_tblSystemAlert_AlertCode"
  ON public."tblSystemAlert" ("AlertCode");

CREATE UNIQUE INDEX "UK_tblSystemConfiguration_ConfigurationGroup_ConfigurationKey"
  ON public."tblSystemConfiguration" ("ConfigurationGroup", "ConfigurationKey");

CREATE UNIQUE INDEX "UK_tblSystemSetting_SettingKey"
  ON public."tblSystemSetting" ("SettingKey");

CREATE UNIQUE INDEX "UK_tblTechnician_TechnicianCode"
  ON public."tblTechnician" ("TechnicianCode");

CREATE UNIQUE INDEX "UK_tblTechnicianAttendance_TechnicianId_AttendanceDate"
  ON public."tblTechnicianAttendance" ("TechnicianId", "AttendanceDate");

CREATE UNIQUE INDEX "UK_tblTechnicianAvailability_TechnicianId_AvailableDate"
  ON public."tblTechnicianAvailability" ("TechnicianId", "AvailableDate");

CREATE UNIQUE INDEX "UK_tblTechnicianPerformanceSummary_TechnicianId_SummaryDate"
  ON public."tblTechnicianPerformanceSummary" ("TechnicianId", "SummaryDate");

CREATE UNIQUE INDEX "UK_tblTechnicianVanStock_TechnicianId_ItemId"
  ON public."tblTechnicianVanStock" ("TechnicianId", "ItemId");

CREATE UNIQUE INDEX "UK_tblTonnage_TonnageCode"
  ON public."tblTonnage" ("TonnageCode");

CREATE UNIQUE INDEX "UK_tblUnitOfMeasure_UnitCode"
  ON public."tblUnitOfMeasure" ("UnitCode");

CREATE UNIQUE INDEX "UK_tblUser_Email"
  ON public."tblUser" ("Email");

CREATE UNIQUE INDEX "UK_tblUser_UserName"
  ON public."tblUser" ("UserName");

CREATE UNIQUE INDEX "UK_tblUserRole_UserId_RoleId"
  ON public."tblUserRole" ("UserId", "RoleId");

CREATE UNIQUE INDEX "UK_tblUserSession_AccessTokenJti"
  ON public."tblUserSession" ("AccessTokenJti");

CREATE UNIQUE INDEX "UK_tblWarehouse_WarehouseCode"
  ON public."tblWarehouse" ("WarehouseCode");

CREATE UNIQUE INDEX "UK_tblWarehouseStock_WarehouseId_ItemId"
  ON public."tblWarehouseStock" ("WarehouseId", "ItemId");

CREATE UNIQUE INDEX "UK_tblWarrantyRule_RuleName"
  ON public."tblWarrantyRule" ("RuleName");

CREATE UNIQUE INDEX "UK_tblZone_ZoneCode"
  ON public."tblZone" ("ZoneCode");

CREATE UNIQUE INDEX "UK_tblZonePincode_ZoneId_Pincode"
  ON public."tblZonePincode" ("ZoneId", "Pincode");

CREATE INDEX "IDX_tblAMCVisitSchedule_ServiceRequestId"
  ON public."tblAMCVisitSchedule" ("ServiceRequestId");

CREATE INDEX "IDX_tblAssignmentLog_CurrentTechnicianId"
  ON public."tblAssignmentLog" ("CurrentTechnicianId");

CREATE INDEX "IDX_tblAssignmentLog_PreviousTechnicianId"
  ON public."tblAssignmentLog" ("PreviousTechnicianId");

CREATE INDEX "IDX_tblBooking_CustomerAddressId"
  ON public."tblBooking" ("CustomerAddressId");

CREATE INDEX "IDX_tblBooking_SlotAvailabilityId"
  ON public."tblBooking" ("SlotAvailabilityId");

CREATE INDEX "IDX_tblBooking_ZoneId"
  ON public."tblBooking" ("ZoneId");

CREATE INDEX "IDX_tblBookingLine_AcTypeId"
  ON public."tblBookingLine" ("AcTypeId");

CREATE INDEX "IDX_tblBookingLine_BookingId"
  ON public."tblBookingLine" ("BookingId");

CREATE INDEX "IDX_tblBookingLine_BrandId"
  ON public."tblBookingLine" ("BrandId");

CREATE INDEX "IDX_tblBookingLine_ServiceId"
  ON public."tblBookingLine" ("ServiceId");

CREATE INDEX "IDX_tblBookingLine_TonnageId"
  ON public."tblBookingLine" ("TonnageId");

CREATE INDEX "IDX_tblCampaign_ServiceId"
  ON public."tblCampaign" ("ServiceId");

CREATE INDEX "IDX_tblCampaign_SlotAvailabilityId"
  ON public."tblCampaign" ("SlotAvailabilityId");

CREATE INDEX "IDX_tblCampaign_ZoneId"
  ON public."tblCampaign" ("ZoneId");

CREATE INDEX "IDX_tblCancellationRecord_ApprovedByUserId"
  ON public."tblCancellationRecord" ("ApprovedByUserId");

CREATE INDEX "IDX_tblCancellationRecord_CancelledByUserId"
  ON public."tblCancellationRecord" ("CancelledByUserId");

CREATE INDEX "IDX_tblCommissioningCertificate_InstallationId"
  ON public."tblCommissioningCertificate" ("InstallationId");

CREATE INDEX "IDX_tblCommissioningCertificate_InstallationOrderId"
  ON public."tblCommissioningCertificate" ("InstallationOrderId");

CREATE INDEX "IDX_tblCustomerAbsentRecord_TechnicianId"
  ON public."tblCustomerAbsentRecord" ("TechnicianId");

CREATE INDEX "IDX_tblCustomerAddress_ZoneId"
  ON public."tblCustomerAddress" ("ZoneId");

CREATE INDEX "IDX_tblCustomerAMC_AmcPlanId"
  ON public."tblCustomerAMC" ("AmcPlanId");

CREATE INDEX "IDX_tblCustomerAMC_JobCardId"
  ON public."tblCustomerAMC" ("JobCardId");

CREATE INDEX "IDX_tblCustomerReview_BookingId"
  ON public."tblCustomerReview" ("BookingId");

CREATE INDEX "IDX_tblCustomerReview_CustomerId"
  ON public."tblCustomerReview" ("CustomerId");

CREATE INDEX "IDX_tblCustomerSignature_JobCardId"
  ON public."tblCustomerSignature" ("JobCardId");

CREATE INDEX "IDX_tblCustomerSignature_JobReportId"
  ON public."tblCustomerSignature" ("JobReportId");

CREATE INDEX "IDX_tblCustomerSignature_ServiceRequestId"
  ON public."tblCustomerSignature" ("ServiceRequestId");

CREATE INDEX "IDX_tblCustomerSignature_TechnicianId"
  ON public."tblCustomerSignature" ("TechnicianId");

CREATE INDEX "IDX_tblHelperAssignment_JobCardId"
  ON public."tblHelperAssignment" ("JobCardId");

CREATE INDEX "IDX_tblHelperAssignment_ServiceRequestId"
  ON public."tblHelperAssignment" ("ServiceRequestId");

CREATE INDEX "IDX_tblHelperAssignment_TechnicianId"
  ON public."tblHelperAssignment" ("TechnicianId");

CREATE INDEX "IDX_tblHelperTaskResponse_HelperTaskChecklistId"
  ON public."tblHelperTaskResponse" ("HelperTaskChecklistId");

CREATE INDEX "IDX_tblInstallationChecklistResponse_InstallationId"
  ON public."tblInstallationChecklistResponse" ("InstallationId");

CREATE INDEX "IDX_tblInstallationLead_AssignedTechnicianId"
  ON public."tblInstallationLead" ("AssignedTechnicianId");

CREATE INDEX "IDX_tblInstallationLead_CustomerAddressId"
  ON public."tblInstallationLead" ("CustomerAddressId");

CREATE INDEX "IDX_tblInstallationLead_LeadId"
  ON public."tblInstallationLead" ("LeadId");

CREATE INDEX "IDX_tblInstallationOrder_CustomerAddressId"
  ON public."tblInstallationOrder" ("CustomerAddressId");

CREATE INDEX "IDX_tblInstallationOrder_CustomerId"
  ON public."tblInstallationOrder" ("CustomerId");

CREATE INDEX "IDX_tblInstallationOrder_LeadId"
  ON public."tblInstallationOrder" ("LeadId");

CREATE INDEX "IDX_tblInstallationOrder_ServiceRequestId"
  ON public."tblInstallationOrder" ("ServiceRequestId");

CREATE INDEX "IDX_tblInstallationSurvey_TechnicianId"
  ON public."tblInstallationSurvey" ("TechnicianId");

CREATE INDEX "IDX_tblInvoiceLine_QuotationLineId"
  ON public."tblInvoiceLine" ("QuotationLineId");

CREATE INDEX "IDX_tblItem_ItemCategoryId"
  ON public."tblItem" ("ItemCategoryId");

CREATE INDEX "IDX_tblItem_SupplierId"
  ON public."tblItem" ("SupplierId");

CREATE INDEX "IDX_tblItem_UnitOfMeasureId"
  ON public."tblItem" ("UnitOfMeasureId");

CREATE INDEX "IDX_tblJobChecklistResponse_ServiceChecklistMasterId"
  ON public."tblJobChecklistResponse" ("ServiceChecklistMasterId");

CREATE INDEX "IDX_tblJobDiagnosis_ComplaintIssueMasterId"
  ON public."tblJobDiagnosis" ("ComplaintIssueMasterId");

CREATE INDEX "IDX_tblJobDiagnosis_DiagnosisResultMasterId"
  ON public."tblJobDiagnosis" ("DiagnosisResultMasterId");

CREATE INDEX "IDX_tblJobPartConsumption_ItemId"
  ON public."tblJobPartConsumption" ("ItemId");

CREATE INDEX "IDX_tblJobPartConsumption_TechnicianId"
  ON public."tblJobPartConsumption" ("TechnicianId");

CREATE INDEX "IDX_tblJobPhoto_JobCardId"
  ON public."tblJobPhoto" ("JobCardId");

CREATE INDEX "IDX_tblJobPhoto_JobReportId"
  ON public."tblJobPhoto" ("JobReportId");

CREATE INDEX "IDX_tblJobPhoto_ServiceRequestId"
  ON public."tblJobPhoto" ("ServiceRequestId");

CREATE INDEX "IDX_tblJobPhoto_TechnicianId"
  ON public."tblJobPhoto" ("TechnicianId");

CREATE INDEX "IDX_tblJobReport_JobCardId"
  ON public."tblJobReport" ("JobCardId");

CREATE INDEX "IDX_tblJobReport_TechnicianId"
  ON public."tblJobReport" ("TechnicianId");

CREATE INDEX "IDX_tblLeadAssignment_PreviousAssignedUserId"
  ON public."tblLeadAssignment" ("PreviousAssignedUserId");

CREATE INDEX "IDX_tblLeadConversion_ServiceRequestId"
  ON public."tblLeadConversion" ("ServiceRequestId");

CREATE INDEX "IDX_tblLeads_AcTypeId"
  ON public."tblLeads" ("AcTypeId");

CREATE INDEX "IDX_tblLeads_AssignedUserId"
  ON public."tblLeads" ("AssignedUserId");

CREATE INDEX "IDX_tblLeads_BrandId"
  ON public."tblLeads" ("BrandId");

CREATE INDEX "IDX_tblLeads_ConvertedBookingId"
  ON public."tblLeads" ("ConvertedBookingId");

CREATE INDEX "IDX_tblLeads_ConvertedServiceRequestId"
  ON public."tblLeads" ("ConvertedServiceRequestId");

CREATE INDEX "IDX_tblLeads_ServiceId"
  ON public."tblLeads" ("ServiceId");

CREATE INDEX "IDX_tblLeads_SlotAvailabilityId"
  ON public."tblLeads" ("SlotAvailabilityId");

CREATE INDEX "IDX_tblLeads_TonnageId"
  ON public."tblLeads" ("TonnageId");

CREATE INDEX "IDX_tblPartsRequest_JobCardId"
  ON public."tblPartsRequest" ("JobCardId");

CREATE INDEX "IDX_tblPartsRequest_TechnicianId"
  ON public."tblPartsRequest" ("TechnicianId");

CREATE INDEX "IDX_tblPartsReturn_ItemId"
  ON public."tblPartsReturn" ("ItemId");

CREATE INDEX "IDX_tblPartsReturn_JobCardId"
  ON public."tblPartsReturn" ("JobCardId");

CREATE INDEX "IDX_tblPartsReturn_SupplierId"
  ON public."tblPartsReturn" ("SupplierId");

CREATE INDEX "IDX_tblPartsReturn_TechnicianId"
  ON public."tblPartsReturn" ("TechnicianId");

CREATE INDEX "IDX_tblPaymentReceipt_InvoiceHeaderId"
  ON public."tblPaymentReceipt" ("InvoiceHeaderId");

CREATE INDEX "IDX_tblPaymentWebhookAttempt_InvoiceHeaderId"
  ON public."tblPaymentWebhookAttempt" ("InvoiceHeaderId");

CREATE INDEX "IDX_tblPurchaseOrder_SupplierId"
  ON public."tblPurchaseOrder" ("SupplierId");

CREATE INDEX "IDX_tblRefundApproval_ApproverUserId"
  ON public."tblRefundApproval" ("ApproverUserId");

CREATE INDEX "IDX_tblRefundRequest_ApprovedByUserId"
  ON public."tblRefundRequest" ("ApprovedByUserId");

CREATE INDEX "IDX_tblRefundRequest_CancellationRecordId"
  ON public."tblRefundRequest" ("CancellationRecordId");

CREATE INDEX "IDX_tblRefundRequest_PaymentTransactionId"
  ON public."tblRefundRequest" ("PaymentTransactionId");

CREATE INDEX "IDX_tblRevisitRequest_CustomerAmcId"
  ON public."tblRevisitRequest" ("CustomerAmcId");

CREATE INDEX "IDX_tblRevisitRequest_CustomerId"
  ON public."tblRevisitRequest" ("CustomerId");

CREATE INDEX "IDX_tblRevisitRequest_OriginalJobCardId"
  ON public."tblRevisitRequest" ("OriginalJobCardId");

CREATE INDEX "IDX_tblRevisitRequest_OriginalServiceRequestId"
  ON public."tblRevisitRequest" ("OriginalServiceRequestId");

CREATE INDEX "IDX_tblRevisitRequest_ServiceRequestId"
  ON public."tblRevisitRequest" ("ServiceRequestId");

CREATE INDEX "IDX_tblService_PricingModelId"
  ON public."tblService" ("PricingModelId");

CREATE INDEX "IDX_tblSiteSurveyReport_InstallationOrderId"
  ON public."tblSiteSurveyReport" ("InstallationOrderId");

CREATE INDEX "IDX_tblSkillAssessment_AssessedByUserId"
  ON public."tblSkillAssessment" ("AssessedByUserId");

CREATE INDEX "IDX_tblSlotAvailability_SlotConfigurationId"
  ON public."tblSlotAvailability" ("SlotConfigurationId");

CREATE INDEX "IDX_tblStockTransaction_JobCardId"
  ON public."tblStockTransaction" ("JobCardId");

CREATE INDEX "IDX_tblStockTransaction_SupplierId"
  ON public."tblStockTransaction" ("SupplierId");

CREATE INDEX "IDX_tblSupportTicket_SupportTicketCategoryId"
  ON public."tblSupportTicket" ("SupportTicketCategoryId");

CREATE INDEX "IDX_tblSupportTicket_SupportTicketPriorityId"
  ON public."tblSupportTicket" ("SupportTicketPriorityId");

CREATE INDEX "IDX_tblTechnician_BaseZoneId"
  ON public."tblTechnician" ("BaseZoneId");

CREATE INDEX "IDX_tblTechnician_UserId"
  ON public."tblTechnician" ("UserId");

CREATE INDEX "IDX_tblTechnicianActivationLog_ActivatedByUserId"
  ON public."tblTechnicianActivationLog" ("ActivatedByUserId");

CREATE INDEX "IDX_tblTechnicianAttendance_ReviewedByUserId"
  ON public."tblTechnicianAttendance" ("ReviewedByUserId");

CREATE INDEX "IDX_tblTechnicianDocument_VerifiedByUserId"
  ON public."tblTechnicianDocument" ("VerifiedByUserId");

CREATE INDEX "IDX_tblTechnicianEarnings_InstallationOrderId"
  ON public."tblTechnicianEarnings" ("InstallationOrderId");

CREATE INDEX "IDX_tblTechnicianEarnings_ServiceRequestId"
  ON public."tblTechnicianEarnings" ("ServiceRequestId");

CREATE INDEX "IDX_tblTechnicianGPSLog_ServiceRequestId"
  ON public."tblTechnicianGPSLog" ("ServiceRequestId");

CREATE INDEX "IDX_tblTechnicianSkillMapping_AcTypeId"
  ON public."tblTechnicianSkillMapping" ("AcTypeId");

CREATE INDEX "IDX_tblTechnicianSkillMapping_ServiceId"
  ON public."tblTechnicianSkillMapping" ("ServiceId");

CREATE INDEX "IDX_tblTechnicianVanStock_ItemId"
  ON public."tblTechnicianVanStock" ("ItemId");

CREATE INDEX "IDX_tblTechnicianZone_ZoneId"
  ON public."tblTechnicianZone" ("ZoneId");

CREATE INDEX "IDX_tblTrainingRecord_TrainerUserId"
  ON public."tblTrainingRecord" ("TrainerUserId");

CREATE INDEX "IDX_tblWarehouseStock_ItemId"
  ON public."tblWarehouseStock" ("ItemId");

CREATE INDEX "IDX_tblWarrantyClaim_CustomerId"
  ON public."tblWarrantyClaim" ("CustomerId");

CREATE INDEX "IDX_tblWarrantyClaim_JobCardId"
  ON public."tblWarrantyClaim" ("JobCardId");

CREATE INDEX "IDX_tblWarrantyClaim_WarrantyRuleId"
  ON public."tblWarrantyClaim" ("WarrantyRuleId");

CREATE INDEX "IDX_tblWarrantyRule_AcTypeId"
  ON public."tblWarrantyRule" ("AcTypeId");

CREATE INDEX "IDX_tblWarrantyRule_BrandId"
  ON public."tblWarrantyRule" ("BrandId");
