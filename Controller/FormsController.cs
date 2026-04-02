using Lucene.Net.Search;
using Lucene.Net.Search.Join;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common;
using Umbraco.Forms.Core.Models;
using Umbraco.Forms.Core.Persistence.Dtos;
using Umbraco.Forms.Core.Services;
using sahanaweb.Constants;
using static Umbraco.Cms.Core.Constants.Conventions;

namespace sahanaweb.Controller
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class FormsController : ControllerBase
    {

        private readonly IFormService _formService;
        private readonly IRecordService _recordService;
        private readonly ILogger<FormsController> _logger;
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly UmbracoHelper _umbraco;


        public FormsController(IFormService formService, IRecordService recordService,
                               ILogger<FormsController> logger, IUmbracoContextAccessor umbracoContextAccessor)
        {
            _formService = formService;
            _recordService = recordService;
            _logger = logger;
            _umbracoContextAccessor = umbracoContextAccessor;
        }
        [HttpGet]
        public IActionResult GetFormDefinition(Guid formId)
        {
            var form = _formService.Get(formId);
            if (form == null)
                return NotFound(new { message = "Form not found" });

            return Ok(form);
        }
        [HttpPost]
        public async Task<IActionResult> Submit([FromBody] FormSubmissionModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Get form
                var form = _formService.Get(model.FormId);
                if (form == null)
                    return NotFound("Form does not exist");

                // Form-level validation
                var validationErrors = new Dictionary<string, string>();

                foreach (var field in form.AllFields)
                {
                    model.Fields.TryGetValue(field.Alias, out var value);
                    var stringValue = value?.ToString()?.Trim();

                    // 1️⃣ Required validation
                    if (field.Mandatory && string.IsNullOrWhiteSpace(stringValue))
                    {
                        validationErrors[field.Alias] = $"{field.Caption} is required.";
                        continue;
                    }
                    // 2️⃣ Regex validation (Email, URL, etc.)
                    if (!string.IsNullOrWhiteSpace(field.RegEx))
                    {
                        if (!Regex.IsMatch(stringValue, field.RegEx))
                        {
                            validationErrors[field.Alias] = $"{field.Caption} is not valid.";
                        }
                    }
                    if (string.IsNullOrWhiteSpace(stringValue))
                        continue; // nothing more to validate

                }


                if (validationErrors.Any())
                    return UnprocessableEntity(new { success = false, errors = validationErrors });

                // Build record
                var record = new Record
                {
                    Form = form.Id, // use form.Id
                    Created = DateTime.Now,
                    Updated = DateTime.Now,
                    Culture = System.Threading.Thread.CurrentThread.CurrentCulture.Name
                };

                foreach (var field in form.AllFields)
                {
                    if (!model.Fields.TryGetValue(field.Alias, out var value))
                        continue;

                    // Make sure value is something Forms expects – usually string
                    var valueObj = value is string ? value : value?.ToString();

                    var recordField = new RecordField(field)
                    {
                        Alias = field.Alias,
                        FieldId = field.Id,
                        Key = Guid.NewGuid(),
                        Record = record.Id,
                        Values = new List<object> { valueObj ?? string.Empty }
                    };

                    record.RecordFields.Add(field.Id, recordField);
                }

                await _recordService.SubmitAsync(record, form);

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Form submission failed");
                return StatusCode(500, "Submission failed");
            }
        }

        public class FormSubmissionModel
        {
            public Guid FormId { get; set; }
            public Dictionary<string, object> Fields { get; set; }
        }

    }

}
