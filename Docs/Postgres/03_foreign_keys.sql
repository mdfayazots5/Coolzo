-- Coolzo PostgreSQL Migration | File 3 | Foreign Keys

ALTER TABLE public."tblAMCVisitSchedule"
  ADD CONSTRAINT "FK_tblAMCVisitSchedule_CustomerAmcId_tblCustomerAMC_CustomerAmcId"
  FOREIGN KEY ("CustomerAmcId")
  REFERENCES public."tblCustomerAMC" ("CustomerAmcId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblAMCVisitSchedule"
  ADD CONSTRAINT "FK_tblAMCVisitSchedule_ServiceRequestId_tblServiceRequest_ServiceRequestId"
  FOREIGN KEY ("ServiceRequestId")
  REFERENCES public."tblServiceRequest" ("ServiceRequestId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblAssignmentLog"
  ADD CONSTRAINT "FK_tblAssignmentLog_CurrentTechnicianId_tblTechnician_TechnicianId"
  FOREIGN KEY ("CurrentTechnicianId")
  REFERENCES public."tblTechnician" ("TechnicianId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblAssignmentLog"
  ADD CONSTRAINT "FK_tblAssignmentLog_PreviousTechnicianId_tblTechnician_TechnicianId"
  FOREIGN KEY ("PreviousTechnicianId")
  REFERENCES public."tblTechnician" ("TechnicianId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblAssignmentLog"
  ADD CONSTRAINT "FK_tblAssignmentLog_ServiceRequestId_tblServiceRequest_ServiceRequestId"
  FOREIGN KEY ("ServiceRequestId")
  REFERENCES public."tblServiceRequest" ("ServiceRequestId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblBillingStatusHistory"
  ADD CONSTRAINT "FK_tblBillingStatusHistory_InvoiceHeaderId_tblInvoiceHeader_InvoiceHeaderId"
  FOREIGN KEY ("InvoiceHeaderId")
  REFERENCES public."tblInvoiceHeader" ("InvoiceHeaderId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblBillingStatusHistory"
  ADD CONSTRAINT "FK_tblBillingStatusHistory_PaymentTransactionId_tblPaymentTransaction_PaymentTransactionId"
  FOREIGN KEY ("PaymentTransactionId")
  REFERENCES public."tblPaymentTransaction" ("PaymentTransactionId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblBillingStatusHistory"
  ADD CONSTRAINT "FK_tblBillingStatusHistory_QuotationHeaderId_tblQuotationHeader_QuotationHeaderId"
  FOREIGN KEY ("QuotationHeaderId")
  REFERENCES public."tblQuotationHeader" ("QuotationHeaderId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblBooking"
  ADD CONSTRAINT "FK_tblBooking_CustomerAddressId_tblCustomerAddress_CustomerAddressId"
  FOREIGN KEY ("CustomerAddressId")
  REFERENCES public."tblCustomerAddress" ("CustomerAddressId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblBooking"
  ADD CONSTRAINT "FK_tblBooking_CustomerId_tblCustomer_CustomerId"
  FOREIGN KEY ("CustomerId")
  REFERENCES public."tblCustomer" ("CustomerId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblBooking"
  ADD CONSTRAINT "FK_tblBooking_SlotAvailabilityId_tblSlotAvailability_SlotAvailabilityId"
  FOREIGN KEY ("SlotAvailabilityId")
  REFERENCES public."tblSlotAvailability" ("SlotAvailabilityId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblBooking"
  ADD CONSTRAINT "FK_tblBooking_ZoneId_tblZone_ZoneId"
  FOREIGN KEY ("ZoneId")
  REFERENCES public."tblZone" ("ZoneId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblBookingLine"
  ADD CONSTRAINT "FK_tblBookingLine_AcTypeId_tblAcType_AcTypeId"
  FOREIGN KEY ("AcTypeId")
  REFERENCES public."tblAcType" ("AcTypeId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblBookingLine"
  ADD CONSTRAINT "FK_tblBookingLine_BookingId_tblBooking_BookingId"
  FOREIGN KEY ("BookingId")
  REFERENCES public."tblBooking" ("BookingId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblBookingLine"
  ADD CONSTRAINT "FK_tblBookingLine_BrandId_tblBrand_BrandId"
  FOREIGN KEY ("BrandId")
  REFERENCES public."tblBrand" ("BrandId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblBookingLine"
  ADD CONSTRAINT "FK_tblBookingLine_ServiceId_tblService_ServiceId"
  FOREIGN KEY ("ServiceId")
  REFERENCES public."tblService" ("ServiceId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblBookingLine"
  ADD CONSTRAINT "FK_tblBookingLine_TonnageId_tblTonnage_TonnageId"
  FOREIGN KEY ("TonnageId")
  REFERENCES public."tblTonnage" ("TonnageId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblBookingStatusHistory"
  ADD CONSTRAINT "FK_tblBookingStatusHistory_BookingId_tblBooking_BookingId"
  FOREIGN KEY ("BookingId")
  REFERENCES public."tblBooking" ("BookingId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblCampaign"
  ADD CONSTRAINT "FK_tblCampaign_ServiceId_tblService_ServiceId"
  FOREIGN KEY ("ServiceId")
  REFERENCES public."tblService" ("ServiceId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblCampaign"
  ADD CONSTRAINT "FK_tblCampaign_SlotAvailabilityId_tblSlotAvailability_SlotAvailabilityId"
  FOREIGN KEY ("SlotAvailabilityId")
  REFERENCES public."tblSlotAvailability" ("SlotAvailabilityId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblCampaign"
  ADD CONSTRAINT "FK_tblCampaign_ZoneId_tblZone_ZoneId"
  FOREIGN KEY ("ZoneId")
  REFERENCES public."tblZone" ("ZoneId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblCancellationRecord"
  ADD CONSTRAINT "FK_tblCancellationRecord_ApprovedByUserId_tblUser_UserId"
  FOREIGN KEY ("ApprovedByUserId")
  REFERENCES public."tblUser" ("UserId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblCancellationRecord"
  ADD CONSTRAINT "FK_tblCancellationRecord_BookingId_tblBooking_BookingId"
  FOREIGN KEY ("BookingId")
  REFERENCES public."tblBooking" ("BookingId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblCancellationRecord"
  ADD CONSTRAINT "FK_tblCancellationRecord_CancelledByUserId_tblUser_UserId"
  FOREIGN KEY ("CancelledByUserId")
  REFERENCES public."tblUser" ("UserId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblCancellationRecord"
  ADD CONSTRAINT "FK_tblCancellationRecord_ServiceRequestId_tblServiceRequest_ServiceRequestId"
  FOREIGN KEY ("ServiceRequestId")
  REFERENCES public."tblServiceRequest" ("ServiceRequestId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblCommissioningCertificate"
  ADD CONSTRAINT "FK_tblCommissioningCertificate_InstallationId_tblInstallationLead_InstallationId"
  FOREIGN KEY ("InstallationId")
  REFERENCES public."tblInstallationLead" ("InstallationId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblCommissioningCertificate"
  ADD CONSTRAINT "FK_tblCommissioningCertificate_InstallationOrderId_tblInstallationOrder_InstallationOrderId"
  FOREIGN KEY ("InstallationOrderId")
  REFERENCES public."tblInstallationOrder" ("InstallationOrderId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblCommunicationPreference"
  ADD CONSTRAINT "FK_tblCommunicationPreference_CustomerId_tblCustomer_CustomerId"
  FOREIGN KEY ("CustomerId")
  REFERENCES public."tblCustomer" ("CustomerId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblComplaintIssueMaster"
  ADD CONSTRAINT "FK_tblComplaintIssueMaster_ServiceId_tblService_ServiceId"
  FOREIGN KEY ("ServiceId")
  REFERENCES public."tblService" ("ServiceId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblCustomer"
  ADD CONSTRAINT "FK_tblCustomer_UserId_tblUser_UserId"
  FOREIGN KEY ("UserId")
  REFERENCES public."tblUser" ("UserId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblCustomerAbsentRecord"
  ADD CONSTRAINT "FK_tblCustomerAbsentRecord_ServiceRequestId_tblServiceRequest_ServiceRequestId"
  FOREIGN KEY ("ServiceRequestId")
  REFERENCES public."tblServiceRequest" ("ServiceRequestId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblCustomerAbsentRecord"
  ADD CONSTRAINT "FK_tblCustomerAbsentRecord_TechnicianId_tblTechnician_TechnicianId"
  FOREIGN KEY ("TechnicianId")
  REFERENCES public."tblTechnician" ("TechnicianId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblCustomerAddress"
  ADD CONSTRAINT "FK_tblCustomerAddress_CustomerId_tblCustomer_CustomerId"
  FOREIGN KEY ("CustomerId")
  REFERENCES public."tblCustomer" ("CustomerId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblCustomerAddress"
  ADD CONSTRAINT "FK_tblCustomerAddress_ZoneId_tblZone_ZoneId"
  FOREIGN KEY ("ZoneId")
  REFERENCES public."tblZone" ("ZoneId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblCustomerAMC"
  ADD CONSTRAINT "FK_tblCustomerAMC_AmcPlanId_tblAMCPlan_AmcPlanId"
  FOREIGN KEY ("AmcPlanId")
  REFERENCES public."tblAMCPlan" ("AmcPlanId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblCustomerAMC"
  ADD CONSTRAINT "FK_tblCustomerAMC_CustomerId_tblCustomer_CustomerId"
  FOREIGN KEY ("CustomerId")
  REFERENCES public."tblCustomer" ("CustomerId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblCustomerAMC"
  ADD CONSTRAINT "FK_tblCustomerAMC_InvoiceHeaderId_tblInvoiceHeader_InvoiceHeaderId"
  FOREIGN KEY ("InvoiceHeaderId")
  REFERENCES public."tblInvoiceHeader" ("InvoiceHeaderId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblCustomerAMC"
  ADD CONSTRAINT "FK_tblCustomerAMC_JobCardId_tblJobCard_JobCardId"
  FOREIGN KEY ("JobCardId")
  REFERENCES public."tblJobCard" ("JobCardId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblCustomerEquipment"
  ADD CONSTRAINT "FK_tblCustomerEquipment_CustomerId_tblCustomer_CustomerId"
  FOREIGN KEY ("CustomerId")
  REFERENCES public."tblCustomer" ("CustomerId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblCustomerReview"
  ADD CONSTRAINT "FK_tblCustomerReview_BookingId_tblBooking_BookingId"
  FOREIGN KEY ("BookingId")
  REFERENCES public."tblBooking" ("BookingId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblCustomerReview"
  ADD CONSTRAINT "FK_tblCustomerReview_CustomerId_tblCustomer_CustomerId"
  FOREIGN KEY ("CustomerId")
  REFERENCES public."tblCustomer" ("CustomerId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblCustomerSignature"
  ADD CONSTRAINT "FK_tblCustomerSignature_JobCardId_tblJobCard_JobCardId"
  FOREIGN KEY ("JobCardId")
  REFERENCES public."tblJobCard" ("JobCardId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblCustomerSignature"
  ADD CONSTRAINT "FK_tblCustomerSignature_JobReportId_tblJobReport_JobReportId"
  FOREIGN KEY ("JobReportId")
  REFERENCES public."tblJobReport" ("JobReportId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblCustomerSignature"
  ADD CONSTRAINT "FK_tblCustomerSignature_ServiceRequestId_tblServiceRequest_ServiceRequestId"
  FOREIGN KEY ("ServiceRequestId")
  REFERENCES public."tblServiceRequest" ("ServiceRequestId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblCustomerSignature"
  ADD CONSTRAINT "FK_tblCustomerSignature_TechnicianId_tblTechnician_TechnicianId"
  FOREIGN KEY ("TechnicianId")
  REFERENCES public."tblTechnician" ("TechnicianId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblHelperAssignment"
  ADD CONSTRAINT "FK_tblHelperAssignment_HelperProfileId_tblHelperProfile_HelperProfileId"
  FOREIGN KEY ("HelperProfileId")
  REFERENCES public."tblHelperProfile" ("HelperProfileId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblHelperAssignment"
  ADD CONSTRAINT "FK_tblHelperAssignment_JobCardId_tblJobCard_JobCardId"
  FOREIGN KEY ("JobCardId")
  REFERENCES public."tblJobCard" ("JobCardId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblHelperAssignment"
  ADD CONSTRAINT "FK_tblHelperAssignment_ServiceRequestId_tblServiceRequest_ServiceRequestId"
  FOREIGN KEY ("ServiceRequestId")
  REFERENCES public."tblServiceRequest" ("ServiceRequestId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblHelperAssignment"
  ADD CONSTRAINT "FK_tblHelperAssignment_TechnicianId_tblTechnician_TechnicianId"
  FOREIGN KEY ("TechnicianId")
  REFERENCES public."tblTechnician" ("TechnicianId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblHelperAttendance"
  ADD CONSTRAINT "FK_tblHelperAttendance_HelperProfileId_tblHelperProfile_HelperProfileId"
  FOREIGN KEY ("HelperProfileId")
  REFERENCES public."tblHelperProfile" ("HelperProfileId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblHelperProfile"
  ADD CONSTRAINT "FK_tblHelperProfile_UserId_tblUser_UserId"
  FOREIGN KEY ("UserId")
  REFERENCES public."tblUser" ("UserId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblHelperTaskResponse"
  ADD CONSTRAINT "FK_tblHelperTaskResponse_HelperAssignmentId_tblHelperAssignment_HelperAssignmentId"
  FOREIGN KEY ("HelperAssignmentId")
  REFERENCES public."tblHelperAssignment" ("HelperAssignmentId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblHelperTaskResponse"
  ADD CONSTRAINT "FK_tblHelperTaskResponse_HelperTaskChecklistId_tblHelperTaskChecklist_HelperTaskChecklistId"
  FOREIGN KEY ("HelperTaskChecklistId")
  REFERENCES public."tblHelperTaskChecklist" ("HelperTaskChecklistId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblInstallationChecklist"
  ADD CONSTRAINT "FK_tblInstallationChecklist_InstallationId_tblInstallationLead_InstallationId"
  FOREIGN KEY ("InstallationId")
  REFERENCES public."tblInstallationLead" ("InstallationId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblInstallationChecklistResponse"
  ADD CONSTRAINT "FK_tblInstallationChecklistResponse_InstallationChecklistId_tblInstallationChecklist_InstallationChecklistId"
  FOREIGN KEY ("InstallationChecklistId")
  REFERENCES public."tblInstallationChecklist" ("InstallationChecklistId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblInstallationChecklistResponse"
  ADD CONSTRAINT "FK_tblInstallationChecklistResponse_InstallationId_tblInstallationLead_InstallationId"
  FOREIGN KEY ("InstallationId")
  REFERENCES public."tblInstallationLead" ("InstallationId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblInstallationLead"
  ADD CONSTRAINT "FK_tblInstallationLead_AssignedTechnicianId_tblTechnician_TechnicianId"
  FOREIGN KEY ("AssignedTechnicianId")
  REFERENCES public."tblTechnician" ("TechnicianId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblInstallationLead"
  ADD CONSTRAINT "FK_tblInstallationLead_CustomerAddressId_tblCustomerAddress_CustomerAddressId"
  FOREIGN KEY ("CustomerAddressId")
  REFERENCES public."tblCustomerAddress" ("CustomerAddressId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblInstallationLead"
  ADD CONSTRAINT "FK_tblInstallationLead_CustomerId_tblCustomer_CustomerId"
  FOREIGN KEY ("CustomerId")
  REFERENCES public."tblCustomer" ("CustomerId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblInstallationLead"
  ADD CONSTRAINT "FK_tblInstallationLead_LeadId_tblLeads_LeadId"
  FOREIGN KEY ("LeadId")
  REFERENCES public."tblLeads" ("LeadId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblInstallationOrder"
  ADD CONSTRAINT "FK_tblInstallationOrder_CustomerAddressId_tblCustomerAddress_CustomerAddressId"
  FOREIGN KEY ("CustomerAddressId")
  REFERENCES public."tblCustomerAddress" ("CustomerAddressId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblInstallationOrder"
  ADD CONSTRAINT "FK_tblInstallationOrder_CustomerId_tblCustomer_CustomerId"
  FOREIGN KEY ("CustomerId")
  REFERENCES public."tblCustomer" ("CustomerId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblInstallationOrder"
  ADD CONSTRAINT "FK_tblInstallationOrder_InstallationId_tblInstallationLead_InstallationId"
  FOREIGN KEY ("InstallationId")
  REFERENCES public."tblInstallationLead" ("InstallationId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblInstallationOrder"
  ADD CONSTRAINT "FK_tblInstallationOrder_InstallationProposalId_tblInstallationProposal_InstallationProposalId"
  FOREIGN KEY ("InstallationProposalId")
  REFERENCES public."tblInstallationProposal" ("InstallationProposalId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblInstallationOrder"
  ADD CONSTRAINT "FK_tblInstallationOrder_LeadId_tblLeads_LeadId"
  FOREIGN KEY ("LeadId")
  REFERENCES public."tblLeads" ("LeadId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblInstallationOrder"
  ADD CONSTRAINT "FK_tblInstallationOrder_ServiceRequestId_tblServiceRequest_ServiceRequestId"
  FOREIGN KEY ("ServiceRequestId")
  REFERENCES public."tblServiceRequest" ("ServiceRequestId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblInstallationOrder"
  ADD CONSTRAINT "FK_tblInstallationOrder_TechnicianId_tblTechnician_TechnicianId"
  FOREIGN KEY ("TechnicianId")
  REFERENCES public."tblTechnician" ("TechnicianId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblInstallationProposal"
  ADD CONSTRAINT "FK_tblInstallationProposal_InstallationId_tblInstallationLead_InstallationId"
  FOREIGN KEY ("InstallationId")
  REFERENCES public."tblInstallationLead" ("InstallationId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblInstallationProposalLine"
  ADD CONSTRAINT "FK_tblInstallationProposalLine_InstallationProposalId_tblInstallationProposal_InstallationProposalId"
  FOREIGN KEY ("InstallationProposalId")
  REFERENCES public."tblInstallationProposal" ("InstallationProposalId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblInstallationStatusHistory"
  ADD CONSTRAINT "FK_tblInstallationStatusHistory_InstallationId_tblInstallationLead_InstallationId"
  FOREIGN KEY ("InstallationId")
  REFERENCES public."tblInstallationLead" ("InstallationId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblInstallationSurvey"
  ADD CONSTRAINT "FK_tblInstallationSurvey_InstallationId_tblInstallationLead_InstallationId"
  FOREIGN KEY ("InstallationId")
  REFERENCES public."tblInstallationLead" ("InstallationId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblInstallationSurvey"
  ADD CONSTRAINT "FK_tblInstallationSurvey_TechnicianId_tblTechnician_TechnicianId"
  FOREIGN KEY ("TechnicianId")
  REFERENCES public."tblTechnician" ("TechnicianId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblInstallationSurveyItem"
  ADD CONSTRAINT "FK_tblInstallationSurveyItem_InstallationSurveyId_tblInstallationSurvey_InstallationSurveyId"
  FOREIGN KEY ("InstallationSurveyId")
  REFERENCES public."tblInstallationSurvey" ("InstallationSurveyId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblInvoiceHeader"
  ADD CONSTRAINT "FK_tblInvoiceHeader_CustomerId_tblCustomer_CustomerId"
  FOREIGN KEY ("CustomerId")
  REFERENCES public."tblCustomer" ("CustomerId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblInvoiceHeader"
  ADD CONSTRAINT "FK_tblInvoiceHeader_QuotationHeaderId_tblQuotationHeader_QuotationHeaderId"
  FOREIGN KEY ("QuotationHeaderId")
  REFERENCES public."tblQuotationHeader" ("QuotationHeaderId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblInvoiceLine"
  ADD CONSTRAINT "FK_tblInvoiceLine_InvoiceHeaderId_tblInvoiceHeader_InvoiceHeaderId"
  FOREIGN KEY ("InvoiceHeaderId")
  REFERENCES public."tblInvoiceHeader" ("InvoiceHeaderId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblInvoiceLine"
  ADD CONSTRAINT "FK_tblInvoiceLine_QuotationLineId_tblQuotationLine_QuotationLineId"
  FOREIGN KEY ("QuotationLineId")
  REFERENCES public."tblQuotationLine" ("QuotationLineId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblItem"
  ADD CONSTRAINT "FK_tblItem_ItemCategoryId_tblItemCategory_ItemCategoryId"
  FOREIGN KEY ("ItemCategoryId")
  REFERENCES public."tblItemCategory" ("ItemCategoryId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblItem"
  ADD CONSTRAINT "FK_tblItem_SupplierId_tblSupplier_SupplierId"
  FOREIGN KEY ("SupplierId")
  REFERENCES public."tblSupplier" ("SupplierId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblItem"
  ADD CONSTRAINT "FK_tblItem_UnitOfMeasureId_tblUnitOfMeasure_UnitOfMeasureId"
  FOREIGN KEY ("UnitOfMeasureId")
  REFERENCES public."tblUnitOfMeasure" ("UnitOfMeasureId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblItemRate"
  ADD CONSTRAINT "FK_tblItemRate_ItemId_tblItem_ItemId"
  FOREIGN KEY ("ItemId")
  REFERENCES public."tblItem" ("ItemId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblJobAttachment"
  ADD CONSTRAINT "FK_tblJobAttachment_JobCardId_tblJobCard_JobCardId"
  FOREIGN KEY ("JobCardId")
  REFERENCES public."tblJobCard" ("JobCardId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblJobCard"
  ADD CONSTRAINT "FK_tblJobCard_ServiceRequestId_tblServiceRequest_ServiceRequestId"
  FOREIGN KEY ("ServiceRequestId")
  REFERENCES public."tblServiceRequest" ("ServiceRequestId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblJobChecklistResponse"
  ADD CONSTRAINT "FK_tblJobChecklistResponse_JobCardId_tblJobCard_JobCardId"
  FOREIGN KEY ("JobCardId")
  REFERENCES public."tblJobCard" ("JobCardId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblJobChecklistResponse"
  ADD CONSTRAINT "FK_tblJobChecklistResponse_ServiceChecklistMasterId_tblServiceChecklistMaster_ServiceChecklistMasterId"
  FOREIGN KEY ("ServiceChecklistMasterId")
  REFERENCES public."tblServiceChecklistMaster" ("ServiceChecklistMasterId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblJobDiagnosis"
  ADD CONSTRAINT "FK_tblJobDiagnosis_ComplaintIssueMasterId_tblComplaintIssueMaster_ComplaintIssueMasterId"
  FOREIGN KEY ("ComplaintIssueMasterId")
  REFERENCES public."tblComplaintIssueMaster" ("ComplaintIssueMasterId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblJobDiagnosis"
  ADD CONSTRAINT "FK_tblJobDiagnosis_DiagnosisResultMasterId_tblDiagnosisResultMaster_DiagnosisResultMasterId"
  FOREIGN KEY ("DiagnosisResultMasterId")
  REFERENCES public."tblDiagnosisResultMaster" ("DiagnosisResultMasterId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblJobDiagnosis"
  ADD CONSTRAINT "FK_tblJobDiagnosis_JobCardId_tblJobCard_JobCardId"
  FOREIGN KEY ("JobCardId")
  REFERENCES public."tblJobCard" ("JobCardId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblJobExecutionNote"
  ADD CONSTRAINT "FK_tblJobExecutionNote_JobCardId_tblJobCard_JobCardId"
  FOREIGN KEY ("JobCardId")
  REFERENCES public."tblJobCard" ("JobCardId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblJobExecutionTimeline"
  ADD CONSTRAINT "FK_tblJobExecutionTimeline_JobCardId_tblJobCard_JobCardId"
  FOREIGN KEY ("JobCardId")
  REFERENCES public."tblJobCard" ("JobCardId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblJobPartConsumption"
  ADD CONSTRAINT "FK_tblJobPartConsumption_ItemId_tblItem_ItemId"
  FOREIGN KEY ("ItemId")
  REFERENCES public."tblItem" ("ItemId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblJobPartConsumption"
  ADD CONSTRAINT "FK_tblJobPartConsumption_JobCardId_tblJobCard_JobCardId"
  FOREIGN KEY ("JobCardId")
  REFERENCES public."tblJobCard" ("JobCardId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblJobPartConsumption"
  ADD CONSTRAINT "FK_tblJobPartConsumption_StockTransactionId_tblStockTransaction_StockTransactionId"
  FOREIGN KEY ("StockTransactionId")
  REFERENCES public."tblStockTransaction" ("StockTransactionId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblJobPartConsumption"
  ADD CONSTRAINT "FK_tblJobPartConsumption_TechnicianId_tblTechnician_TechnicianId"
  FOREIGN KEY ("TechnicianId")
  REFERENCES public."tblTechnician" ("TechnicianId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblJobPhoto"
  ADD CONSTRAINT "FK_tblJobPhoto_JobCardId_tblJobCard_JobCardId"
  FOREIGN KEY ("JobCardId")
  REFERENCES public."tblJobCard" ("JobCardId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblJobPhoto"
  ADD CONSTRAINT "FK_tblJobPhoto_JobReportId_tblJobReport_JobReportId"
  FOREIGN KEY ("JobReportId")
  REFERENCES public."tblJobReport" ("JobReportId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblJobPhoto"
  ADD CONSTRAINT "FK_tblJobPhoto_ServiceRequestId_tblServiceRequest_ServiceRequestId"
  FOREIGN KEY ("ServiceRequestId")
  REFERENCES public."tblServiceRequest" ("ServiceRequestId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblJobPhoto"
  ADD CONSTRAINT "FK_tblJobPhoto_TechnicianId_tblTechnician_TechnicianId"
  FOREIGN KEY ("TechnicianId")
  REFERENCES public."tblTechnician" ("TechnicianId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblJobReport"
  ADD CONSTRAINT "FK_tblJobReport_JobCardId_tblJobCard_JobCardId"
  FOREIGN KEY ("JobCardId")
  REFERENCES public."tblJobCard" ("JobCardId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblJobReport"
  ADD CONSTRAINT "FK_tblJobReport_ServiceRequestId_tblServiceRequest_ServiceRequestId"
  FOREIGN KEY ("ServiceRequestId")
  REFERENCES public."tblServiceRequest" ("ServiceRequestId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblJobReport"
  ADD CONSTRAINT "FK_tblJobReport_TechnicianId_tblTechnician_TechnicianId"
  FOREIGN KEY ("TechnicianId")
  REFERENCES public."tblTechnician" ("TechnicianId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblLeadAssignment"
  ADD CONSTRAINT "FK_tblLeadAssignment_AssignedUserId_tblUser_UserId"
  FOREIGN KEY ("AssignedUserId")
  REFERENCES public."tblUser" ("UserId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblLeadAssignment"
  ADD CONSTRAINT "FK_tblLeadAssignment_LeadId_tblLeads_LeadId"
  FOREIGN KEY ("LeadId")
  REFERENCES public."tblLeads" ("LeadId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblLeadAssignment"
  ADD CONSTRAINT "FK_tblLeadAssignment_PreviousAssignedUserId_tblUser_UserId"
  FOREIGN KEY ("PreviousAssignedUserId")
  REFERENCES public."tblUser" ("UserId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblLeadConversion"
  ADD CONSTRAINT "FK_tblLeadConversion_BookingId_tblBooking_BookingId"
  FOREIGN KEY ("BookingId")
  REFERENCES public."tblBooking" ("BookingId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblLeadConversion"
  ADD CONSTRAINT "FK_tblLeadConversion_LeadId_tblLeads_LeadId"
  FOREIGN KEY ("LeadId")
  REFERENCES public."tblLeads" ("LeadId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblLeadConversion"
  ADD CONSTRAINT "FK_tblLeadConversion_ServiceRequestId_tblServiceRequest_ServiceRequestId"
  FOREIGN KEY ("ServiceRequestId")
  REFERENCES public."tblServiceRequest" ("ServiceRequestId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblLeadNote"
  ADD CONSTRAINT "FK_tblLeadNote_LeadId_tblLeads_LeadId"
  FOREIGN KEY ("LeadId")
  REFERENCES public."tblLeads" ("LeadId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblLeads"
  ADD CONSTRAINT "FK_tblLeads_AcTypeId_tblAcType_AcTypeId"
  FOREIGN KEY ("AcTypeId")
  REFERENCES public."tblAcType" ("AcTypeId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblLeads"
  ADD CONSTRAINT "FK_tblLeads_AssignedUserId_tblUser_UserId"
  FOREIGN KEY ("AssignedUserId")
  REFERENCES public."tblUser" ("UserId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblLeads"
  ADD CONSTRAINT "FK_tblLeads_BrandId_tblBrand_BrandId"
  FOREIGN KEY ("BrandId")
  REFERENCES public."tblBrand" ("BrandId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblLeads"
  ADD CONSTRAINT "FK_tblLeads_ConvertedBookingId_tblBooking_BookingId"
  FOREIGN KEY ("ConvertedBookingId")
  REFERENCES public."tblBooking" ("BookingId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblLeads"
  ADD CONSTRAINT "FK_tblLeads_ConvertedServiceRequestId_tblServiceRequest_ServiceRequestId"
  FOREIGN KEY ("ConvertedServiceRequestId")
  REFERENCES public."tblServiceRequest" ("ServiceRequestId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblLeads"
  ADD CONSTRAINT "FK_tblLeads_ServiceId_tblService_ServiceId"
  FOREIGN KEY ("ServiceId")
  REFERENCES public."tblService" ("ServiceId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblLeads"
  ADD CONSTRAINT "FK_tblLeads_SlotAvailabilityId_tblSlotAvailability_SlotAvailabilityId"
  FOREIGN KEY ("SlotAvailabilityId")
  REFERENCES public."tblSlotAvailability" ("SlotAvailabilityId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblLeads"
  ADD CONSTRAINT "FK_tblLeads_TonnageId_tblTonnage_TonnageId"
  FOREIGN KEY ("TonnageId")
  REFERENCES public."tblTonnage" ("TonnageId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblLeadStatusHistory"
  ADD CONSTRAINT "FK_tblLeadStatusHistory_LeadId_tblLeads_LeadId"
  FOREIGN KEY ("LeadId")
  REFERENCES public."tblLeads" ("LeadId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblOtpVerification"
  ADD CONSTRAINT "FK_tblOtpVerification_UserId_tblUser_UserId"
  FOREIGN KEY ("UserId")
  REFERENCES public."tblUser" ("UserId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblPartsRequest"
  ADD CONSTRAINT "FK_tblPartsRequest_JobCardId_tblJobCard_JobCardId"
  FOREIGN KEY ("JobCardId")
  REFERENCES public."tblJobCard" ("JobCardId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblPartsRequest"
  ADD CONSTRAINT "FK_tblPartsRequest_ServiceRequestId_tblServiceRequest_ServiceRequestId"
  FOREIGN KEY ("ServiceRequestId")
  REFERENCES public."tblServiceRequest" ("ServiceRequestId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblPartsRequest"
  ADD CONSTRAINT "FK_tblPartsRequest_TechnicianId_tblTechnician_TechnicianId"
  FOREIGN KEY ("TechnicianId")
  REFERENCES public."tblTechnician" ("TechnicianId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblPartsRequestItem"
  ADD CONSTRAINT "FK_tblPartsRequestItem_ItemId_tblItem_ItemId"
  FOREIGN KEY ("ItemId")
  REFERENCES public."tblItem" ("ItemId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblPartsRequestItem"
  ADD CONSTRAINT "FK_tblPartsRequestItem_PartsRequestId_tblPartsRequest_PartsRequestId"
  FOREIGN KEY ("PartsRequestId")
  REFERENCES public."tblPartsRequest" ("PartsRequestId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblPartsReturn"
  ADD CONSTRAINT "FK_tblPartsReturn_ItemId_tblItem_ItemId"
  FOREIGN KEY ("ItemId")
  REFERENCES public."tblItem" ("ItemId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblPartsReturn"
  ADD CONSTRAINT "FK_tblPartsReturn_JobCardId_tblJobCard_JobCardId"
  FOREIGN KEY ("JobCardId")
  REFERENCES public."tblJobCard" ("JobCardId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblPartsReturn"
  ADD CONSTRAINT "FK_tblPartsReturn_SupplierId_tblSupplier_SupplierId"
  FOREIGN KEY ("SupplierId")
  REFERENCES public."tblSupplier" ("SupplierId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblPartsReturn"
  ADD CONSTRAINT "FK_tblPartsReturn_TechnicianId_tblTechnician_TechnicianId"
  FOREIGN KEY ("TechnicianId")
  REFERENCES public."tblTechnician" ("TechnicianId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblPaymentReceipt"
  ADD CONSTRAINT "FK_tblPaymentReceipt_InvoiceHeaderId_tblInvoiceHeader_InvoiceHeaderId"
  FOREIGN KEY ("InvoiceHeaderId")
  REFERENCES public."tblInvoiceHeader" ("InvoiceHeaderId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblPaymentReceipt"
  ADD CONSTRAINT "FK_tblPaymentReceipt_PaymentTransactionId_tblPaymentTransaction_PaymentTransactionId"
  FOREIGN KEY ("PaymentTransactionId")
  REFERENCES public."tblPaymentTransaction" ("PaymentTransactionId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblPaymentTransaction"
  ADD CONSTRAINT "FK_tblPaymentTransaction_InvoiceHeaderId_tblInvoiceHeader_InvoiceHeaderId"
  FOREIGN KEY ("InvoiceHeaderId")
  REFERENCES public."tblInvoiceHeader" ("InvoiceHeaderId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblPaymentWebhookAttempt"
  ADD CONSTRAINT "FK_tblPaymentWebhookAttempt_InvoiceHeaderId_tblInvoiceHeader_InvoiceHeaderId"
  FOREIGN KEY ("InvoiceHeaderId")
  REFERENCES public."tblInvoiceHeader" ("InvoiceHeaderId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblPurchaseOrder"
  ADD CONSTRAINT "FK_tblPurchaseOrder_SupplierId_tblSupplier_SupplierId"
  FOREIGN KEY ("SupplierId")
  REFERENCES public."tblSupplier" ("SupplierId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblPurchaseOrderItem"
  ADD CONSTRAINT "FK_tblPurchaseOrderItem_ItemId_tblItem_ItemId"
  FOREIGN KEY ("ItemId")
  REFERENCES public."tblItem" ("ItemId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblPurchaseOrderItem"
  ADD CONSTRAINT "FK_tblPurchaseOrderItem_PurchaseOrderId_tblPurchaseOrder_PurchaseOrderId"
  FOREIGN KEY ("PurchaseOrderId")
  REFERENCES public."tblPurchaseOrder" ("PurchaseOrderId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblQuotationHeader"
  ADD CONSTRAINT "FK_tblQuotationHeader_CustomerId_tblCustomer_CustomerId"
  FOREIGN KEY ("CustomerId")
  REFERENCES public."tblCustomer" ("CustomerId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblQuotationHeader"
  ADD CONSTRAINT "FK_tblQuotationHeader_JobCardId_tblJobCard_JobCardId"
  FOREIGN KEY ("JobCardId")
  REFERENCES public."tblJobCard" ("JobCardId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblQuotationLine"
  ADD CONSTRAINT "FK_tblQuotationLine_QuotationHeaderId_tblQuotationHeader_QuotationHeaderId"
  FOREIGN KEY ("QuotationHeaderId")
  REFERENCES public."tblQuotationHeader" ("QuotationHeaderId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblRefreshToken"
  ADD CONSTRAINT "FK_tblRefreshToken_UserId_tblUser_UserId"
  FOREIGN KEY ("UserId")
  REFERENCES public."tblUser" ("UserId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblRefreshToken"
  ADD CONSTRAINT "FK_tblRefreshToken_UserSessionId_tblUserSession_UserSessionId"
  FOREIGN KEY ("UserSessionId")
  REFERENCES public."tblUserSession" ("UserSessionId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblRefundApproval"
  ADD CONSTRAINT "FK_tblRefundApproval_ApproverUserId_tblUser_UserId"
  FOREIGN KEY ("ApproverUserId")
  REFERENCES public."tblUser" ("UserId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblRefundApproval"
  ADD CONSTRAINT "FK_tblRefundApproval_RefundRequestId_tblRefundRequest_RefundRequestId"
  FOREIGN KEY ("RefundRequestId")
  REFERENCES public."tblRefundRequest" ("RefundRequestId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblRefundRequest"
  ADD CONSTRAINT "FK_tblRefundRequest_ApprovedByUserId_tblUser_UserId"
  FOREIGN KEY ("ApprovedByUserId")
  REFERENCES public."tblUser" ("UserId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblRefundRequest"
  ADD CONSTRAINT "FK_tblRefundRequest_CancellationRecordId_tblCancellationRecord_CancellationRecordId"
  FOREIGN KEY ("CancellationRecordId")
  REFERENCES public."tblCancellationRecord" ("CancellationRecordId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblRefundRequest"
  ADD CONSTRAINT "FK_tblRefundRequest_InvoiceHeaderId_tblInvoiceHeader_InvoiceHeaderId"
  FOREIGN KEY ("InvoiceHeaderId")
  REFERENCES public."tblInvoiceHeader" ("InvoiceHeaderId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblRefundRequest"
  ADD CONSTRAINT "FK_tblRefundRequest_PaymentTransactionId_tblPaymentTransaction_PaymentTransactionId"
  FOREIGN KEY ("PaymentTransactionId")
  REFERENCES public."tblPaymentTransaction" ("PaymentTransactionId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblRefundStatusHistory"
  ADD CONSTRAINT "FK_tblRefundStatusHistory_RefundRequestId_tblRefundRequest_RefundRequestId"
  FOREIGN KEY ("RefundRequestId")
  REFERENCES public."tblRefundRequest" ("RefundRequestId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblRevisitRequest"
  ADD CONSTRAINT "FK_tblRevisitRequest_BookingId_tblBooking_BookingId"
  FOREIGN KEY ("BookingId")
  REFERENCES public."tblBooking" ("BookingId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblRevisitRequest"
  ADD CONSTRAINT "FK_tblRevisitRequest_CustomerAmcId_tblCustomerAMC_CustomerAmcId"
  FOREIGN KEY ("CustomerAmcId")
  REFERENCES public."tblCustomerAMC" ("CustomerAmcId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblRevisitRequest"
  ADD CONSTRAINT "FK_tblRevisitRequest_CustomerId_tblCustomer_CustomerId"
  FOREIGN KEY ("CustomerId")
  REFERENCES public."tblCustomer" ("CustomerId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblRevisitRequest"
  ADD CONSTRAINT "FK_tblRevisitRequest_OriginalJobCardId_tblJobCard_JobCardId"
  FOREIGN KEY ("OriginalJobCardId")
  REFERENCES public."tblJobCard" ("JobCardId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblRevisitRequest"
  ADD CONSTRAINT "FK_tblRevisitRequest_OriginalServiceRequestId_tblServiceRequest_ServiceRequestId"
  FOREIGN KEY ("OriginalServiceRequestId")
  REFERENCES public."tblServiceRequest" ("ServiceRequestId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblRevisitRequest"
  ADD CONSTRAINT "FK_tblRevisitRequest_ServiceRequestId_tblServiceRequest_ServiceRequestId"
  FOREIGN KEY ("ServiceRequestId")
  REFERENCES public."tblServiceRequest" ("ServiceRequestId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblRevisitRequest"
  ADD CONSTRAINT "FK_tblRevisitRequest_WarrantyClaimId_tblWarrantyClaim_WarrantyClaimId"
  FOREIGN KEY ("WarrantyClaimId")
  REFERENCES public."tblWarrantyClaim" ("WarrantyClaimId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblRolePermission"
  ADD CONSTRAINT "FK_tblRolePermission_PermissionId_tblPermission_PermissionId"
  FOREIGN KEY ("PermissionId")
  REFERENCES public."tblPermission" ("PermissionId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblRolePermission"
  ADD CONSTRAINT "FK_tblRolePermission_RoleId_tblRole_RoleId"
  FOREIGN KEY ("RoleId")
  REFERENCES public."tblRole" ("RoleId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblService"
  ADD CONSTRAINT "FK_tblService_PricingModelId_tblPricingModel_PricingModelId"
  FOREIGN KEY ("PricingModelId")
  REFERENCES public."tblPricingModel" ("PricingModelId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblService"
  ADD CONSTRAINT "FK_tblService_ServiceCategoryId_tblServiceCategory_ServiceCategoryId"
  FOREIGN KEY ("ServiceCategoryId")
  REFERENCES public."tblServiceCategory" ("ServiceCategoryId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblServiceChecklistMaster"
  ADD CONSTRAINT "FK_tblServiceChecklistMaster_ServiceId_tblService_ServiceId"
  FOREIGN KEY ("ServiceId")
  REFERENCES public."tblService" ("ServiceId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblServiceRequest"
  ADD CONSTRAINT "FK_tblServiceRequest_BookingId_tblBooking_BookingId"
  FOREIGN KEY ("BookingId")
  REFERENCES public."tblBooking" ("BookingId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblServiceRequestAssignment"
  ADD CONSTRAINT "FK_tblServiceRequestAssignment_ServiceRequestId_tblServiceRequest_ServiceRequestId"
  FOREIGN KEY ("ServiceRequestId")
  REFERENCES public."tblServiceRequest" ("ServiceRequestId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblServiceRequestAssignment"
  ADD CONSTRAINT "FK_tblServiceRequestAssignment_TechnicianId_tblTechnician_TechnicianId"
  FOREIGN KEY ("TechnicianId")
  REFERENCES public."tblTechnician" ("TechnicianId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblServiceRequestStatusHistory"
  ADD CONSTRAINT "FK_tblServiceRequestStatusHistory_ServiceRequestId_tblServiceRequest_ServiceRequestId"
  FOREIGN KEY ("ServiceRequestId")
  REFERENCES public."tblServiceRequest" ("ServiceRequestId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblSiteSurveyReport"
  ADD CONSTRAINT "FK_tblSiteSurveyReport_InstallationOrderId_tblInstallationOrder_InstallationOrderId"
  FOREIGN KEY ("InstallationOrderId")
  REFERENCES public."tblInstallationOrder" ("InstallationOrderId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblSkillAssessment"
  ADD CONSTRAINT "FK_tblSkillAssessment_AssessedByUserId_tblUser_UserId"
  FOREIGN KEY ("AssessedByUserId")
  REFERENCES public."tblUser" ("UserId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblSkillAssessment"
  ADD CONSTRAINT "FK_tblSkillAssessment_TechnicianId_tblTechnician_TechnicianId"
  FOREIGN KEY ("TechnicianId")
  REFERENCES public."tblTechnician" ("TechnicianId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblSlotAvailability"
  ADD CONSTRAINT "FK_tblSlotAvailability_SlotConfigurationId_tblSlotConfiguration_SlotConfigurationId"
  FOREIGN KEY ("SlotConfigurationId")
  REFERENCES public."tblSlotConfiguration" ("SlotConfigurationId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblSlotAvailability"
  ADD CONSTRAINT "FK_tblSlotAvailability_ZoneId_tblZone_ZoneId"
  FOREIGN KEY ("ZoneId")
  REFERENCES public."tblZone" ("ZoneId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblSlotConfiguration"
  ADD CONSTRAINT "FK_tblSlotConfiguration_ZoneId_tblZone_ZoneId"
  FOREIGN KEY ("ZoneId")
  REFERENCES public."tblZone" ("ZoneId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblStockTransaction"
  ADD CONSTRAINT "FK_tblStockTransaction_ItemId_tblItem_ItemId"
  FOREIGN KEY ("ItemId")
  REFERENCES public."tblItem" ("ItemId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblStockTransaction"
  ADD CONSTRAINT "FK_tblStockTransaction_JobCardId_tblJobCard_JobCardId"
  FOREIGN KEY ("JobCardId")
  REFERENCES public."tblJobCard" ("JobCardId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblStockTransaction"
  ADD CONSTRAINT "FK_tblStockTransaction_SupplierId_tblSupplier_SupplierId"
  FOREIGN KEY ("SupplierId")
  REFERENCES public."tblSupplier" ("SupplierId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblStockTransaction"
  ADD CONSTRAINT "FK_tblStockTransaction_TechnicianId_tblTechnician_TechnicianId"
  FOREIGN KEY ("TechnicianId")
  REFERENCES public."tblTechnician" ("TechnicianId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblStockTransaction"
  ADD CONSTRAINT "FK_tblStockTransaction_WarehouseId_tblWarehouse_WarehouseId"
  FOREIGN KEY ("WarehouseId")
  REFERENCES public."tblWarehouse" ("WarehouseId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblSupportTicket"
  ADD CONSTRAINT "FK_tblSupportTicket_CustomerId_tblCustomer_CustomerId"
  FOREIGN KEY ("CustomerId")
  REFERENCES public."tblCustomer" ("CustomerId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblSupportTicket"
  ADD CONSTRAINT "FK_tblSupportTicket_SupportTicketCategoryId_tblSupportTicketCategory_SupportTicketCategoryId"
  FOREIGN KEY ("SupportTicketCategoryId")
  REFERENCES public."tblSupportTicketCategory" ("SupportTicketCategoryId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblSupportTicket"
  ADD CONSTRAINT "FK_tblSupportTicket_SupportTicketPriorityId_tblSupportTicketPriority_SupportTicketPriorityId"
  FOREIGN KEY ("SupportTicketPriorityId")
  REFERENCES public."tblSupportTicketPriority" ("SupportTicketPriorityId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblSupportTicketAssignment"
  ADD CONSTRAINT "FK_tblSupportTicketAssignment_AssignedUserId_tblUser_UserId"
  FOREIGN KEY ("AssignedUserId")
  REFERENCES public."tblUser" ("UserId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblSupportTicketAssignment"
  ADD CONSTRAINT "FK_tblSupportTicketAssignment_SupportTicketId_tblSupportTicket_SupportTicketId"
  FOREIGN KEY ("SupportTicketId")
  REFERENCES public."tblSupportTicket" ("SupportTicketId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblSupportTicketEscalation"
  ADD CONSTRAINT "FK_tblSupportTicketEscalation_SupportTicketId_tblSupportTicket_SupportTicketId"
  FOREIGN KEY ("SupportTicketId")
  REFERENCES public."tblSupportTicket" ("SupportTicketId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblSupportTicketLink"
  ADD CONSTRAINT "FK_tblSupportTicketLink_SupportTicketId_tblSupportTicket_SupportTicketId"
  FOREIGN KEY ("SupportTicketId")
  REFERENCES public."tblSupportTicket" ("SupportTicketId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblSupportTicketReply"
  ADD CONSTRAINT "FK_tblSupportTicketReply_SupportTicketId_tblSupportTicket_SupportTicketId"
  FOREIGN KEY ("SupportTicketId")
  REFERENCES public."tblSupportTicket" ("SupportTicketId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblSupportTicketStatusHistory"
  ADD CONSTRAINT "FK_tblSupportTicketStatusHistory_SupportTicketId_tblSupportTicket_SupportTicketId"
  FOREIGN KEY ("SupportTicketId")
  REFERENCES public."tblSupportTicket" ("SupportTicketId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblTechnician"
  ADD CONSTRAINT "FK_tblTechnician_BaseZoneId_tblZone_ZoneId"
  FOREIGN KEY ("BaseZoneId")
  REFERENCES public."tblZone" ("ZoneId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblTechnician"
  ADD CONSTRAINT "FK_tblTechnician_UserId_tblUser_UserId"
  FOREIGN KEY ("UserId")
  REFERENCES public."tblUser" ("UserId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblTechnicianActivationLog"
  ADD CONSTRAINT "FK_tblTechnicianActivationLog_ActivatedByUserId_tblUser_UserId"
  FOREIGN KEY ("ActivatedByUserId")
  REFERENCES public."tblUser" ("UserId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblTechnicianActivationLog"
  ADD CONSTRAINT "FK_tblTechnicianActivationLog_TechnicianId_tblTechnician_TechnicianId"
  FOREIGN KEY ("TechnicianId")
  REFERENCES public."tblTechnician" ("TechnicianId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblTechnicianAttendance"
  ADD CONSTRAINT "FK_tblTechnicianAttendance_ReviewedByUserId_tblUser_UserId"
  FOREIGN KEY ("ReviewedByUserId")
  REFERENCES public."tblUser" ("UserId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblTechnicianAttendance"
  ADD CONSTRAINT "FK_tblTechnicianAttendance_TechnicianId_tblTechnician_TechnicianId"
  FOREIGN KEY ("TechnicianId")
  REFERENCES public."tblTechnician" ("TechnicianId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblTechnicianAvailability"
  ADD CONSTRAINT "FK_tblTechnicianAvailability_TechnicianId_tblTechnician_TechnicianId"
  FOREIGN KEY ("TechnicianId")
  REFERENCES public."tblTechnician" ("TechnicianId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblTechnicianDocument"
  ADD CONSTRAINT "FK_tblTechnicianDocument_TechnicianId_tblTechnician_TechnicianId"
  FOREIGN KEY ("TechnicianId")
  REFERENCES public."tblTechnician" ("TechnicianId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblTechnicianDocument"
  ADD CONSTRAINT "FK_tblTechnicianDocument_VerifiedByUserId_tblUser_UserId"
  FOREIGN KEY ("VerifiedByUserId")
  REFERENCES public."tblUser" ("UserId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblTechnicianEarnings"
  ADD CONSTRAINT "FK_tblTechnicianEarnings_InstallationOrderId_tblInstallationOrder_InstallationOrderId"
  FOREIGN KEY ("InstallationOrderId")
  REFERENCES public."tblInstallationOrder" ("InstallationOrderId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblTechnicianEarnings"
  ADD CONSTRAINT "FK_tblTechnicianEarnings_ServiceRequestId_tblServiceRequest_ServiceRequestId"
  FOREIGN KEY ("ServiceRequestId")
  REFERENCES public."tblServiceRequest" ("ServiceRequestId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblTechnicianEarnings"
  ADD CONSTRAINT "FK_tblTechnicianEarnings_TechnicianId_tblTechnician_TechnicianId"
  FOREIGN KEY ("TechnicianId")
  REFERENCES public."tblTechnician" ("TechnicianId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblTechnicianGPSLog"
  ADD CONSTRAINT "FK_tblTechnicianGPSLog_ServiceRequestId_tblServiceRequest_ServiceRequestId"
  FOREIGN KEY ("ServiceRequestId")
  REFERENCES public."tblServiceRequest" ("ServiceRequestId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblTechnicianGPSLog"
  ADD CONSTRAINT "FK_tblTechnicianGPSLog_TechnicianId_tblTechnician_TechnicianId"
  FOREIGN KEY ("TechnicianId")
  REFERENCES public."tblTechnician" ("TechnicianId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblTechnicianPerformanceSummary"
  ADD CONSTRAINT "FK_tblTechnicianPerformanceSummary_TechnicianId_tblTechnician_TechnicianId"
  FOREIGN KEY ("TechnicianId")
  REFERENCES public."tblTechnician" ("TechnicianId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblTechnicianSkill"
  ADD CONSTRAINT "FK_tblTechnicianSkill_TechnicianId_tblTechnician_TechnicianId"
  FOREIGN KEY ("TechnicianId")
  REFERENCES public."tblTechnician" ("TechnicianId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblTechnicianSkillMapping"
  ADD CONSTRAINT "FK_tblTechnicianSkillMapping_AcTypeId_tblAcType_AcTypeId"
  FOREIGN KEY ("AcTypeId")
  REFERENCES public."tblAcType" ("AcTypeId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblTechnicianSkillMapping"
  ADD CONSTRAINT "FK_tblTechnicianSkillMapping_ServiceId_tblService_ServiceId"
  FOREIGN KEY ("ServiceId")
  REFERENCES public."tblService" ("ServiceId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblTechnicianSkillMapping"
  ADD CONSTRAINT "FK_tblTechnicianSkillMapping_TechnicianId_tblTechnician_TechnicianId"
  FOREIGN KEY ("TechnicianId")
  REFERENCES public."tblTechnician" ("TechnicianId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblTechnicianVanStock"
  ADD CONSTRAINT "FK_tblTechnicianVanStock_ItemId_tblItem_ItemId"
  FOREIGN KEY ("ItemId")
  REFERENCES public."tblItem" ("ItemId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblTechnicianVanStock"
  ADD CONSTRAINT "FK_tblTechnicianVanStock_TechnicianId_tblTechnician_TechnicianId"
  FOREIGN KEY ("TechnicianId")
  REFERENCES public."tblTechnician" ("TechnicianId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblTechnicianZone"
  ADD CONSTRAINT "FK_tblTechnicianZone_TechnicianId_tblTechnician_TechnicianId"
  FOREIGN KEY ("TechnicianId")
  REFERENCES public."tblTechnician" ("TechnicianId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblTechnicianZone"
  ADD CONSTRAINT "FK_tblTechnicianZone_ZoneId_tblZone_ZoneId"
  FOREIGN KEY ("ZoneId")
  REFERENCES public."tblZone" ("ZoneId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblTrainingRecord"
  ADD CONSTRAINT "FK_tblTrainingRecord_TechnicianId_tblTechnician_TechnicianId"
  FOREIGN KEY ("TechnicianId")
  REFERENCES public."tblTechnician" ("TechnicianId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblTrainingRecord"
  ADD CONSTRAINT "FK_tblTrainingRecord_TrainerUserId_tblUser_UserId"
  FOREIGN KEY ("TrainerUserId")
  REFERENCES public."tblUser" ("UserId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblUserPasswordHistory"
  ADD CONSTRAINT "FK_tblUserPasswordHistory_UserId_tblUser_UserId"
  FOREIGN KEY ("UserId")
  REFERENCES public."tblUser" ("UserId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblUserRole"
  ADD CONSTRAINT "FK_tblUserRole_RoleId_tblRole_RoleId"
  FOREIGN KEY ("RoleId")
  REFERENCES public."tblRole" ("RoleId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblUserRole"
  ADD CONSTRAINT "FK_tblUserRole_UserId_tblUser_UserId"
  FOREIGN KEY ("UserId")
  REFERENCES public."tblUser" ("UserId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblUserSession"
  ADD CONSTRAINT "FK_tblUserSession_UserId_tblUser_UserId"
  FOREIGN KEY ("UserId")
  REFERENCES public."tblUser" ("UserId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblWarehouseStock"
  ADD CONSTRAINT "FK_tblWarehouseStock_ItemId_tblItem_ItemId"
  FOREIGN KEY ("ItemId")
  REFERENCES public."tblItem" ("ItemId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblWarehouseStock"
  ADD CONSTRAINT "FK_tblWarehouseStock_WarehouseId_tblWarehouse_WarehouseId"
  FOREIGN KEY ("WarehouseId")
  REFERENCES public."tblWarehouse" ("WarehouseId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblWarrantyClaim"
  ADD CONSTRAINT "FK_tblWarrantyClaim_CustomerId_tblCustomer_CustomerId"
  FOREIGN KEY ("CustomerId")
  REFERENCES public."tblCustomer" ("CustomerId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblWarrantyClaim"
  ADD CONSTRAINT "FK_tblWarrantyClaim_InvoiceHeaderId_tblInvoiceHeader_InvoiceHeaderId"
  FOREIGN KEY ("InvoiceHeaderId")
  REFERENCES public."tblInvoiceHeader" ("InvoiceHeaderId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblWarrantyClaim"
  ADD CONSTRAINT "FK_tblWarrantyClaim_JobCardId_tblJobCard_JobCardId"
  FOREIGN KEY ("JobCardId")
  REFERENCES public."tblJobCard" ("JobCardId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblWarrantyClaim"
  ADD CONSTRAINT "FK_tblWarrantyClaim_WarrantyRuleId_tblWarrantyRule_WarrantyRuleId"
  FOREIGN KEY ("WarrantyRuleId")
  REFERENCES public."tblWarrantyRule" ("WarrantyRuleId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblWarrantyRule"
  ADD CONSTRAINT "FK_tblWarrantyRule_AcTypeId_tblAcType_AcTypeId"
  FOREIGN KEY ("AcTypeId")
  REFERENCES public."tblAcType" ("AcTypeId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblWarrantyRule"
  ADD CONSTRAINT "FK_tblWarrantyRule_BrandId_tblBrand_BrandId"
  FOREIGN KEY ("BrandId")
  REFERENCES public."tblBrand" ("BrandId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblWarrantyRule"
  ADD CONSTRAINT "FK_tblWarrantyRule_ServiceId_tblService_ServiceId"
  FOREIGN KEY ("ServiceId")
  REFERENCES public."tblService" ("ServiceId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE public."tblZonePincode"
  ADD CONSTRAINT "FK_tblZonePincode_ZoneId_tblZone_ZoneId"
  FOREIGN KEY ("ZoneId")
  REFERENCES public."tblZone" ("ZoneId")
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;
