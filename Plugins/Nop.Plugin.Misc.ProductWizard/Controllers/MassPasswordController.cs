using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Host;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Messages;
using Nop.Services.Customers;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Web.Framework.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.ProductWizard.Controllers
{
    public class MassPasswordController : BasePluginController
    {
        private readonly ICustomerService _customerService;
        private readonly CustomerSettings _customerSettings;
        private readonly IQueuedEmailService _queuedEmailService;
        private readonly IEventPublisher _eventPublisher;
        private readonly IWorkflowMessageService _wfService;
        private readonly IStoreContext _storeContext; 
        private readonly IMessageTemplateService _messageTemplateService;
        private readonly IEmailAccountService _emailAccountService;
        private readonly EmailAccountSettings _emailAccountSettings;
        private readonly IMessageTokenProvider _messageTokenProvider;
        private readonly ITokenizer _tokenizer;
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="customerService">Customer service</param>
        /// <param name="customerSettings">Customer settings</param>
        public MassPasswordController(ICustomerService customerService,
            CustomerSettings customerSettings,
            IQueuedEmailService queuedEmailService,
            IEventPublisher eventPublisher,
            IWorkflowMessageService wfService,
            IStoreContext storeContext,
            IMessageTemplateService _messageTemplateService,
            IEmailAccountService emailAccountService,
            EmailAccountSettings emailAccountSettings,
            IMessageTokenProvider messageTokenProvider,
            ITokenizer tokenizer)
        {
            this._tokenizer = tokenizer;
            this._messageTokenProvider = messageTokenProvider;
            this._emailAccountService = emailAccountService;
            this._messageTemplateService = _messageTemplateService;
            this._storeContext = storeContext;
            this._wfService = wfService;
            this._queuedEmailService = queuedEmailService;
            this._eventPublisher = eventPublisher;
            this._customerService = customerService;
            this._customerSettings = customerSettings;
            this._emailAccountSettings = emailAccountSettings;
        }

        /// <summary>
        /// Executes a task
        /// </summary>
        public string Execute([FromQuery(Name = "roles")] int[] roles)
        {

            var store = _storeContext.CurrentStore;
            //load language by specified ID
            var languageId = 1;

            //var messageTemplate = _messageTemplateService.GetMessageTemplateByName(MessageTemplateSystemNames.CustomerForceChangePasswordMessage, 0);
            var messageTemplate = _messageTemplateService.GetMessageTemplateById(44);

            //no template found
            if (messageTemplate == null)
                return "messageTemplate is null";

            //ensure it's active
            var isActive = messageTemplate.IsActive;
            if (!isActive)
                return "messageTemplate is not active";


            var emailAccountId = messageTemplate.GetLocalized(mt => mt.EmailAccountId, languageId);
            //some 0 validation (for localizable "Email account" dropdownlist which saves 0 if "Standard" value is chosen)
            if (emailAccountId == 0)
                emailAccountId = messageTemplate.EmailAccountId;

            var emailAccount = _emailAccountService.GetEmailAccountById(emailAccountId);
            if (emailAccount == null)
                emailAccount = _emailAccountService.GetEmailAccountById(_emailAccountSettings.DefaultEmailAccountId);
            if (emailAccount == null)
                emailAccount = _emailAccountService.GetAllEmailAccounts().FirstOrDefault();


            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);

            //var customers = _customerService.GetAllCustomers(customerRoleIds: new int[] { 1, 2, 3 });
            var customers = _customerService.GetAllCustomers(customerRoleIds: roles);
            foreach (var customer in customers)
            {
                _messageTokenProvider.AddCustomerTokens(tokens, customer);

                //event notification
                _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

                var toEmail = customer.Email;
                var toName = customer.GetFullName();


                if (string.IsNullOrEmpty(toEmail))
                    continue;

                //return SendNotification(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
                if (messageTemplate == null)
                    throw new ArgumentNullException(nameof(messageTemplate));

                if (emailAccount == null)
                    throw new ArgumentNullException(nameof(emailAccount));

                //retrieve localized message template data
                var bcc = messageTemplate.GetLocalized(mt => mt.BccEmailAddresses, languageId);

                var subject = messageTemplate.GetLocalized(mt => mt.Subject, languageId);
                var body = messageTemplate.GetLocalized(mt => mt.Body, languageId);

                //Replace subject and body tokens 
                var subjectReplaced = _tokenizer.Replace(subject, tokens, false);
                var bodyReplaced = _tokenizer.Replace(body, tokens, true);

                //limit name length
                toName = CommonHelper.EnsureMaximumLength(toName, 300);

                var email = new QueuedEmail
                {
                    Priority = QueuedEmailPriority.High,
                    From = emailAccount.Email,
                    FromName = emailAccount.DisplayName,
                    To = toEmail,
                    ToName = toName,
                    //ReplyTo = replyToEmailAddress,
                    //ReplyToName = replyToName,
                    CC = string.Empty,
                    Bcc = bcc,
                    Subject = subjectReplaced,
                    Body = bodyReplaced,
                    //AttachmentFilePath = attachmentFilePath,
                    //AttachmentFileName = attachmentFileName,
                    AttachedDownloadId = messageTemplate.AttachedDownloadId,
                    CreatedOnUtc = DateTime.UtcNow,
                    EmailAccountId = emailAccount.Id,
                    DontSendBeforeDateUtc = !messageTemplate.DelayBeforeSend.HasValue ? null
                        : (DateTime?)(DateTime.UtcNow + TimeSpan.FromHours(messageTemplate.DelayPeriod.ToHours(messageTemplate.DelayBeforeSend.Value)))
                };

                _queuedEmailService.InsertQueuedEmail(email);
            }


            return "Mass change Password email process completed";
            //var olderThanMinutes = _customerSettings.DeleteGuestTaskOlderThanMinutes;
            //// Default value in case 0 is returned.  0 would effectively disable this service and harm performance.
            //olderThanMinutes = olderThanMinutes == 0 ? 1440 : olderThanMinutes;

            //_customerService.DeleteGuestCustomers(null, DateTime.UtcNow.AddMinutes(-olderThanMinutes), true);
        }
    }
}
