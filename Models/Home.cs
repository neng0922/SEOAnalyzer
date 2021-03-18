using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace SitecoreSEOAnalyzer.Models
{
    public class Home
    {
        [Url]
        public string Text { get; set; }

        public bool StopwordCheck { get; set; } = true;

        public bool LinkCheck { get; set; } = true;

        public Dictionary<string, int> PageOccurrences { get; set; } = new Dictionary<string, int>();

        public Dictionary<string, int> MetaOccurrences { get; set; } = new Dictionary<string, int>();

        public Dictionary<string, int> ExternalLinks { get; set; } = new Dictionary<string, int>();
    }

    //public class HomeValidator : AbstractValidator<Home>
    //{
    //    public HomeValidator()
    //    {
    //        RuleFor(x => x.Text)
    //            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _)).When(x => !string.IsNullOrEmpty(x.Text))
    //            .WithMessage("Please enter a valid URL");
    //    }
    //}
}
