
using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Kit.Forms.ComponentDataAnnotations
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class Equals : DataTypeAttribute
    {
        private readonly string ComparePropertyName;
        public Equals(string ComparePropertyName) : base(DataType.Custom)
        {
            this.ComparePropertyName = ComparePropertyName;
        }
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            string[] MemberNames = new string[]
            {
                validationContext.MemberName
            };
            Type type = validationContext.ObjectInstance.GetType();
            PropertyInfo property = type.GetProperty(ComparePropertyName);
            if (property is null)
            {
                throw new Exception($"Property {ComparePropertyName} not found on type {type}");
            }
            object compare_value = property.GetValue(validationContext.ObjectInstance);

            if (value.Equals(compare_value))
            {
                return ValidationResult.Success;
            }
            return new ValidationResult(this.ErrorMessage, MemberNames);
        }
    }


}
