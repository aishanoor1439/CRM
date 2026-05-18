namespace ExcellOnServices.Models
{
    public class PaymentDecorator
    {
        // ==================== 1. INTERFACE ====================
        public interface IPaymentCalculator
        {
            decimal Calculate(decimal amount);
            string GetDescription();
        }

        // ==================== 2. BASE CALCULATOR ====================
        public class BasePaymentCalculator : IPaymentCalculator
        {
            public decimal Calculate(decimal amount)
            {
                return amount;
            }

            public string GetDescription()
            {
                return "Base Payment";
            }
        }

        // ==================== 3. ABSTRACT DECORATOR ====================
        public abstract class PaymentDecoratorBase : IPaymentCalculator
        {
            protected IPaymentCalculator _calculator;

            protected PaymentDecoratorBase(IPaymentCalculator calculator)
            {
                _calculator = calculator;
            }

            public virtual decimal Calculate(decimal amount)
            {
                return _calculator.Calculate(amount);
            }

            public virtual string GetDescription()
            {
                return _calculator.GetDescription();
            }
        }

        // ==================== 4. CUSTOM CHARGE DECORATOR ====================
        public class CustomChargeDecorator : PaymentDecoratorBase
        {
            private decimal _customAmount;

            public CustomChargeDecorator(IPaymentCalculator calculator, decimal customAmount)
                : base(calculator)
            {
                _customAmount = customAmount;
            }

            public override decimal Calculate(decimal amount)
            {
                return base.Calculate(amount) + _customAmount;
            }

            public override string GetDescription()
            {
                return $"{_calculator.GetDescription()} + Custom Charge (${_customAmount})";
            }
        }

        // ==================== 5. PROCESSING FEE DECORATOR ====================
        public class ProcessingFeeDecorator : PaymentDecoratorBase
        {
            private decimal _fixedFee;

            public ProcessingFeeDecorator(IPaymentCalculator calculator, decimal fixedFee)
                : base(calculator)
            {
                _fixedFee = fixedFee;
            }

            public override decimal Calculate(decimal amount)
            {
                return base.Calculate(amount) + _fixedFee;
            }

            public override string GetDescription()
            {
                return $"{_calculator.GetDescription()} + Processing Fee (${_fixedFee})";
            }
        }

        // ==================== 6. TAX DECORATOR ====================
        public class TaxDecorator : PaymentDecoratorBase
        {
            private decimal _taxPercentage;

            public TaxDecorator(IPaymentCalculator calculator, decimal taxPercentage)
                : base(calculator)
            {
                _taxPercentage = taxPercentage;
            }

            public override decimal Calculate(decimal amount)
            {
                decimal taxAmount = amount * (_taxPercentage / 100);
                return base.Calculate(amount) + taxAmount;
            }

            public override string GetDescription()
            {
                return $"{_calculator.GetDescription()} + Tax ({_taxPercentage}%)";
            }
        }

        // ==================== 7. TIP DECORATOR ====================
        public class TipDecorator : PaymentDecoratorBase
        {
            private decimal _tipAmount;

            public TipDecorator(IPaymentCalculator calculator, decimal tipAmount)
                : base(calculator)
            {
                _tipAmount = tipAmount;
            }

            public override decimal Calculate(decimal amount)
            {
                return base.Calculate(amount) + _tipAmount;
            }

            public override string GetDescription()
            {
                return $"{_calculator.GetDescription()} + Tip (${_tipAmount})";
            }
        }
    }
}