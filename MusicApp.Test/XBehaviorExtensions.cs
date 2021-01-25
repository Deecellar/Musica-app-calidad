using System;
using Xbehave;
using Xbehave.Sdk;

namespace MusicApp.Test
{
    public static class XBehaviorExtensions
    {
        public static IStepBuilder Given(string text, Action body) => $"Dado {text}".Step(body);
        public static IStepBuilder And(string text, Action body) => $"Y {text}".Step(body);
        public static IStepBuilder But(string text, Action body) => $"Pero {text}".Step(body);
        public static IStepBuilder When(string text, Action body) => $"Cuando {text}".Step(body);
        public static IStepBuilder Then(string text, Action body) => $"Entonces {text}".Step(body);
        public static IStepBuilder Asterisk(string text, Action body) => $" * {text}".Step(body);
        public static IStepBuilder Step(this string text, Action body) => text.x(body);
    }
}