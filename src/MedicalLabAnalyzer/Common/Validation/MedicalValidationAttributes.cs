using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.RegularExpressions;

namespace MedicalLabAnalyzer.Common.Validation
{
    /// <summary>
    /// WHO 2021 compliant semen analysis parameter validation
    /// </summary>
    public class WHO2021ParameterAttribute : ValidationAttribute
    {
        private readonly string _parameterType;
        private readonly double _min;
        private readonly double _max;

        public WHO2021ParameterAttribute(string parameterType, double min, double max)
        {
            _parameterType = parameterType;
            _min = min;
            _max = max;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null) return ValidationResult.Success;

            if (!double.TryParse(value.ToString(), out double doubleValue))
            {
                return new ValidationResult($"قيمة {_parameterType} غير صالحة - Invalid {_parameterType} value");
            }

            if (double.IsNaN(doubleValue) || double.IsInfinity(doubleValue))
            {
                return new ValidationResult($"قيمة {_parameterType} غير صالحة - Invalid {_parameterType} value");
            }

            if (doubleValue < _min || doubleValue > _max)
            {
                return new ValidationResult($"{_parameterType} خارج النطاق المرجعي WHO 2021 ({_min:F1} - {_max:F1}) - {_parameterType} outside WHO 2021 reference range ({_min:F1} - {_max:F1})");
            }

            return ValidationResult.Success;
        }
    }

    /// <summary>
    /// Hematology reference range validation
    /// </summary>
    public class HematologyRangeAttribute : ValidationAttribute
    {
        private readonly double _min;
        private readonly double _max;
        private readonly string _unit;
        private readonly bool _genderSpecific;

        public HematologyRangeAttribute(double min, double max, string unit, bool genderSpecific = false)
        {
            _min = min;
            _max = max;
            _unit = unit;
            _genderSpecific = genderSpecific;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null) return ValidationResult.Success;

            if (!double.TryParse(value.ToString(), out double doubleValue))
            {
                return new ValidationResult($"القيمة غير صالحة - Invalid value");
            }

            if (double.IsNaN(doubleValue) || double.IsInfinity(doubleValue))
            {
                return new ValidationResult($"القيمة غير صالحة - Invalid value");
            }

            if (doubleValue < 0)
            {
                return new ValidationResult($"القيمة لا يمكن أن تكون سالبة - Value cannot be negative");
            }

            if (doubleValue < _min || doubleValue > _max)
            {
                var genderNote = _genderSpecific ? " (قد تختلف حسب الجنس - may vary by gender)" : "";
                return new ValidationResult($"القيمة خارج النطاق المرجعي ({_min:F1} - {_max:F1} {_unit}){genderNote} - Value outside reference range ({_min:F1} - {_max:F1} {_unit}){genderNote}");
            }

            return ValidationResult.Success;
        }
    }

    /// <summary>
    /// Percentage validation (0-100%)
    /// </summary>
    public class PercentageAttribute : ValidationAttribute
    {
        public PercentageAttribute()
        {
            ErrorMessage = "النسبة يجب أن تكون بين 0 و 100% - Percentage must be between 0 and 100%";
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null) return ValidationResult.Success;

            if (!double.TryParse(value.ToString(), out double doubleValue))
            {
                return new ValidationResult("النسبة غير صالحة - Invalid percentage value");
            }

            if (double.IsNaN(doubleValue) || double.IsInfinity(doubleValue))
            {
                return new ValidationResult("النسبة غير صالحة - Invalid percentage value");
            }

            if (doubleValue < 0 || doubleValue > 100)
            {
                return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success;
        }
    }

    /// <summary>
    /// Age validation for medical contexts
    /// </summary>
    public class MedicalAgeAttribute : ValidationAttribute
    {
        private readonly int _minAge;
        private readonly int _maxAge;

        public MedicalAgeAttribute(int minAge = 0, int maxAge = 150)
        {
            _minAge = minAge;
            _maxAge = maxAge;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null) return ValidationResult.Success;

            if (!int.TryParse(value.ToString(), out int age))
            {
                return new ValidationResult("العمر غير صالح - Invalid age");
            }

            if (age < _minAge || age > _maxAge)
            {
                return new ValidationResult($"العمر يجب أن يكون بين {_minAge} و {_maxAge} سنة - Age must be between {_minAge} and {_maxAge} years");
            }

            return ValidationResult.Success;
        }
    }

    /// <summary>
    /// Saudi phone number validation
    /// </summary>
    public class SaudiPhoneAttribute : ValidationAttribute
    {
        public SaudiPhoneAttribute()
        {
            ErrorMessage = "رقم الهاتف السعودي غير صالح - Invalid Saudi phone number";
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                return ValidationResult.Success; // Phone is optional

            string phoneNumber = value.ToString();
            
            // Remove common formatting
            var cleanPhone = Regex.Replace(phoneNumber, @"[\s\-\(\)]", "");

            // Saudi phone number patterns
            if (!Regex.IsMatch(cleanPhone, @"^(\+966|966|0)?[5][0-9]{8}$"))
            {
                return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success;
        }
    }

    /// <summary>
    /// Saudi National ID validation
    /// </summary>
    public class SaudiNationalIdAttribute : ValidationAttribute
    {
        public SaudiNationalIdAttribute()
        {
            ErrorMessage = "رقم الهوية الوطنية السعودية غير صالح - Invalid Saudi National ID";
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                return ValidationResult.Success; // National ID might be optional

            string nationalId = value.ToString();

            if (!Regex.IsMatch(nationalId, @"^[12]\d{9}$"))
            {
                return new ValidationResult(ErrorMessage);
            }

            // Additional checksum validation for Saudi National ID
            if (!IsValidSaudiNationalId(nationalId))
            {
                return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success;
        }

        private bool IsValidSaudiNationalId(string nationalId)
        {
            if (nationalId.Length != 10) return false;

            int sum = 0;
            for (int i = 0; i < 9; i++)
            {
                int digit = int.Parse(nationalId[i].ToString());
                if (i % 2 == 0)
                {
                    digit *= 2;
                    if (digit > 9)
                        digit = (digit / 10) + (digit % 10);
                }
                sum += digit;
            }

            int checkDigit = (10 - (sum % 10)) % 10;
            return checkDigit == int.Parse(nationalId[9].ToString());
        }
    }

    /// <summary>
    /// Medical gender validation
    /// </summary>
    public class MedicalGenderAttribute : ValidationAttribute
    {
        private static readonly string[] ValidGenders = { "M", "F", "Male", "Female", "ذكر", "أنثى" };

        public MedicalGenderAttribute()
        {
            ErrorMessage = "الجنس غير صالح (M, F, Male, Female, ذكر, أنثى) - Invalid gender (M, F, Male, Female, ذكر, أنثى)";
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return new ValidationResult("الجنس مطلوب - Gender is required");
            }

            string gender = value.ToString();

            if (!Array.Exists(ValidGenders, g => g.Equals(gender, StringComparison.OrdinalIgnoreCase)))
            {
                return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success;
        }
    }

    /// <summary>
    /// Urine color validation
    /// </summary>
    public class UrineColorAttribute : ValidationAttribute
    {
        private static readonly string[] ValidColors = { 
            "Yellow", "Pale Yellow", "Dark Yellow", "Amber", "Red", "Brown", "Clear",
            "أصفر", "أصفر فاتح", "أصفر داكن", "كهرماني", "أحمر", "بني", "صافي"
        };

        public UrineColorAttribute()
        {
            ErrorMessage = "لون البول غير صالح - Invalid urine color";
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                return ValidationResult.Success;

            string color = value.ToString();

            if (!Array.Exists(ValidColors, c => c.Equals(color, StringComparison.OrdinalIgnoreCase)))
            {
                return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success;
        }
    }

    /// <summary>
    /// Medical test status validation
    /// </summary>
    public class MedicalTestStatusAttribute : ValidationAttribute
    {
        private static readonly string[] ValidStatuses = { 
            "Pending", "InProgress", "Completed", "Failed", "Cancelled",
            "قيد الانتظار", "جاري", "مكتمل", "فاشل", "ملغي"
        };

        public MedicalTestStatusAttribute()
        {
            ErrorMessage = "حالة الفحص غير صالحة - Invalid test status";
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return new ValidationResult("حالة الفحص مطلوبة - Test status is required");
            }

            string status = value.ToString();

            if (!Array.Exists(ValidStatuses, s => s.Equals(status, StringComparison.OrdinalIgnoreCase)))
            {
                return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success;
        }
    }

    /// <summary>
    /// Positive/Negative result validation
    /// </summary>
    public class PositiveNegativeAttribute : ValidationAttribute
    {
        private static readonly string[] ValidResults = { 
            "Positive", "Negative", "إيجابي", "سلبي"
        };

        public PositiveNegativeAttribute()
        {
            ErrorMessage = "النتيجة يجب أن تكون إيجابية أو سلبية - Result must be positive or negative";
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                return ValidationResult.Success;

            string result = value.ToString();

            if (!Array.Exists(ValidResults, r => r.Equals(result, StringComparison.OrdinalIgnoreCase)))
            {
                return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success;
        }
    }

    /// <summary>
    /// Microscopic examination quantity validation (None, Few, Moderate, Many)
    /// </summary>
    public class MicroscopicQuantityAttribute : ValidationAttribute
    {
        private static readonly string[] ValidQuantities = { 
            "None", "Few", "Moderate", "Many", "لا يوجد", "قليل", "متوسط", "كثير"
        };

        public MicroscopicQuantityAttribute()
        {
            ErrorMessage = "الكمية غير صالحة (لا يوجد، قليل، متوسط، كثير) - Invalid quantity (None, Few, Moderate, Many)";
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                return ValidationResult.Success;

            string quantity = value.ToString();

            if (!Array.Exists(ValidQuantities, q => q.Equals(quantity, StringComparison.OrdinalIgnoreCase)))
            {
                return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success;
        }
    }

    /// <summary>
    /// pH validation for biological samples
    /// </summary>
    public class BiologicalPHAttribute : ValidationAttribute
    {
        private readonly double _minPH;
        private readonly double _maxPH;

        public BiologicalPHAttribute(double minPH = 0.0, double maxPH = 14.0)
        {
            _minPH = minPH;
            _maxPH = maxPH;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null) return ValidationResult.Success;

            if (!double.TryParse(value.ToString(), out double pH))
            {
                return new ValidationResult("قيمة الأس الهيدروجيني غير صالحة - Invalid pH value");
            }

            if (double.IsNaN(pH) || double.IsInfinity(pH))
            {
                return new ValidationResult("قيمة الأس الهيدروجيني غير صالحة - Invalid pH value");
            }

            if (pH < _minPH || pH > _maxPH)
            {
                return new ValidationResult($"الأس الهيدروجيني خارج النطاق الطبيعي ({_minPH:F1} - {_maxPH:F1}) - pH outside normal range ({_minPH:F1} - {_maxPH:F1})");
            }

            return ValidationResult.Success;
        }
    }

    /// <summary>
    /// Temperature validation for medical contexts (Celsius)
    /// </summary>
    public class MedicalTemperatureAttribute : ValidationAttribute
    {
        public MedicalTemperatureAttribute()
        {
            ErrorMessage = "درجة الحرارة خارج النطاق الطبيعي (15-50°C) - Temperature outside normal range (15-50°C)";
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null) return ValidationResult.Success;

            if (!double.TryParse(value.ToString(), out double temperature))
            {
                return new ValidationResult("درجة الحرارة غير صالحة - Invalid temperature value");
            }

            if (double.IsNaN(temperature) || double.IsInfinity(temperature))
            {
                return new ValidationResult("درجة الحرارة غير صالحة - Invalid temperature value");
            }

            if (temperature < 15 || temperature > 50)
            {
                return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success;
        }
    }

    /// <summary>
    /// Required Arabic and English text validation
    /// </summary>
    public class RequiredBilingualAttribute : ValidationAttribute
    {
        public RequiredBilingualAttribute()
        {
            ErrorMessage = "هذا الحقل مطلوب - This field is required";
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success;
        }
    }
}