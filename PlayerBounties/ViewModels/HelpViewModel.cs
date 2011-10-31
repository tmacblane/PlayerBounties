using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using PlayerBounties.Models;

namespace PlayerBounties.ViewModels
{
    public class HelpViewModel
    {
        #region Type specifice properties

        [Required(ErrorMessage = "Email address is required.")]
        [Display(Name = "Email Address")]
        [StringLength(100, ErrorMessage = "The {0} must be less than {1} characters.")]
        public string EmailAddress
        {
            get;
            set;
        }

        [Required(ErrorMessage = "Subject is required.")]
        [Display(Name = "Subject")]
        [StringLength(255, ErrorMessage = "The {0} must be less than {1} characters.")]
        public string SubjectLine
        { 
            get; 
            set; 
        }

        [Required(ErrorMessage = "Message is required.")]
        [DataType(DataType.MultilineText)]
        [StringLength(4000, ErrorMessage = "The {0} must be less than {1} characters.")]
        public string Message 
        { 
            get; 
            set; 
        }

        #endregion
    }
}