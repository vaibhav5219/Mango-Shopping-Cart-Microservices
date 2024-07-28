﻿using System.ComponentModel.DataAnnotations;
namespace Mango.Web.Utility
{
    public class MaxFileSizeAttribute : ValidationAttribute
    {
        private readonly int _maxFileSize;
        public MaxFileSizeAttribute(int maxFileSize)
        {
            _maxFileSize = maxFileSize;
        }
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var file = value as IFormFile;
            if (file != null)
            {
                var extension = Path.GetExtension(file.FileName);
                if (file.Length > (_maxFileSize * 1024 * 1024))
                {
                    return new ValidationResult($"Maximum allowed size is {_maxFileSize} MB.");
                }
            }
            return ValidationResult.Success;
        }
    }
}
