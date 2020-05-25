using System.Collections.Generic;

namespace FillObjectionableReasonTable.Constants
{
    public class ObjectionableReason
    {
        public static readonly Dictionary<string, string> ObjectionableReasons = new Dictionary<string, string>()
        {
            {"NudityOrSexualActivity", "Nudity or sexual activity"},
            {"HateSpeechOrSymbols", "Hate speech or symbols"},
            {"ViolenceOrDangerousOrganizations", "Violence or dangerous organizations"},
            {"SaleOfIllegalOrRegularGoods", "Sale of illegal or regular goods"},
            {"BullyingOrHarassment", "Bullying or harassment"},
            {"IntellectualPropertyViolation", "Intellectual property violation"},
            {"SuicideOrSelfInjury", "Suicide or self-injury"},
            {"ScamOrFraud", "Scam or fraud"},
            {"FalseInformation", "False information"}
        };
    }
}