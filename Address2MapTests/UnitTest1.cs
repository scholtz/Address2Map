using Address2Map.BusinessController;
using Address2Map.Model;
using Address2Map.Repository;
using Address2MapTests.NUnitTestCiscoService;
using Microsoft.Extensions.DependencyInjection;

namespace Address2MapTests
{
    public class Tests
    {
        public MockWebApp web;
        public HttpClient client;

        [SetUp]
        public void Setup()
        {
            web = new MockWebApp();
            client = web.CreateClient();
            var rujan = web.Services.GetService<RuianRepository>();
            rujan?.ProcessCSV(File.ReadAllBytes("Data/20220930_OB_554782_ADR.csv"));
        }

        [Test]
        public void ConversionNabreziTest()
        {
            var scope = web?.Services?.CreateScope();
            var addrController = scope?.ServiceProvider?.GetService(typeof(AddressBusinessController)) as AddressBusinessController;
            Assert.That(addrController, Is.Not.Null);
            var input = @"Rašínovo nábř. - sudá č. 28-50";
            var ret = addrController.ProcessText2Output(554782, input);
            Assert.That(ret.Input.Trim(), Is.EqualTo(input));
            Assert.That(ret.Output.Trim(), Is.EqualTo("Rašínovo nábřeží - sudá č. 28-50"));
            Assert.That(ret.Notes.Trim(), Is.EqualTo("We suggest to change street name"));
        }
        [Test]
        public void ConversionDashTest()
        {
            var scope = web?.Services?.CreateScope();
            var addrController = scope?.ServiceProvider?.GetService(typeof(AddressBusinessController)) as AddressBusinessController;
            Assert.That(addrController, Is.Not.Null);
            char chDash = '\u2013';

            var input = @$"Zlatá ulička u Daliborky {chDash} sudá č. 28–50";
            var ret = addrController.ProcessText2Output(554782, input);
            Assert.That(ret.Input.Trim(), Is.EqualTo(input));
            Assert.That(ret.Output.Trim(), Is.EqualTo("Zlatá ulička u Daliborky - sudá č. 28-50"));
            Assert.That(ret.Notes.Trim(), Is.EqualTo("UTF Dash has been replaced with hyphen"));
        }
        [Test]
        public void DoesNotExistsTest()
        {
            var scope = web?.Services?.CreateScope();
            var addrController = scope?.ServiceProvider?.GetService(typeof(AddressBusinessController)) as AddressBusinessController;
            Assert.That(addrController, Is.Not.Null);

            var input = @$"DoesNotExists";
            var ret = addrController.ProcessText2Output(554782, input);
            Assert.That(ret.Input.Trim(), Is.EqualTo(input));
            Assert.That(ret.Output.Trim(), Is.EqualTo("DoesNotExists"));
            Assert.That(ret.Notes.Trim(), Is.EqualTo("We had trouble finding street DoesNotExists"));
        }
        [Test]
        public void CalculateDataPointsTest()
        {
            var scope = web?.Services?.CreateScope();
            var addrController = scope?.ServiceProvider?.GetService(typeof(AddressBusinessController)) as AddressBusinessController;
            Assert.That(addrController, Is.Not.Null);

            var input = @$"Zlatá ulička u Daliborky";
            var ret = addrController.ProcessText2DataPoints(554782, input);
            Assert.That(ret.SelectMany(i => i).Count(), Is.EqualTo(27));
            Assert.That(ret.Count(), Is.EqualTo(1));
        }
        [Test]
        public void CalculateDataPointsTwoAreasTest()
        {
            var scope = web?.Services?.CreateScope();
            var addrController = scope?.ServiceProvider?.GetService(typeof(AddressBusinessController)) as AddressBusinessController;
            Assert.That(addrController, Is.Not.Null);

            var input = $"!Area1\nZlatá ulička u Daliborky\n!Area2\nZlatá ulička u Daliborky\n";
            var ret = addrController.ProcessText2DataPoints(554782, input);
            Assert.That(ret.Count(), Is.EqualTo(2));
            Assert.That(ret.First().Count(), Is.EqualTo(27));
            Assert.That(ret.Last().Count(), Is.EqualTo(27));
        }

        [Test]
        public void ValidityPatterFailTest()
        {
            var scope = web?.Services?.CreateScope();
            var addrController = scope?.ServiceProvider?.GetService(typeof(AddressBusinessController)) as AddressBusinessController;
            Assert.That(addrController, Is.Not.Null);

            var input = @$"Zlatá ulička u Daliborky - 123";
            var ret = addrController.ProcessText2Output(554782, input);
            Assert.That(ret.Input.Trim(), Is.EqualTo(input));
            Assert.That(ret.Output.Trim(), Is.EqualTo("Zlatá ulička u Daliborky - 123"));
            Assert.That(ret.Notes.Trim(), Is.EqualTo("Format of the input is incorrect"));
        }
        [Test]
        public void ValidityPatterPassTest()
        {
            var scope = web?.Services?.CreateScope();
            var addrController = scope?.ServiceProvider?.GetService(typeof(AddressBusinessController)) as AddressBusinessController;
            Assert.That(addrController, Is.Not.Null);

            var input = @$"Zlatá ulička u Daliborky - sudá č. 28-50";
            var ret = addrController.ProcessText2Output(554782, input);
            Assert.That(ret.Input.Trim(), Is.EqualTo(input));
            Assert.That(ret.Output.Trim(), Is.EqualTo("Zlatá ulička u Daliborky - sudá č. 28-50"));
            Assert.That(ret.Notes.Trim(), Is.Empty);
        }
        [Test]
        public void ValidityPatterPassWithCommentsTest()
        {
            var scope = web?.Services?.CreateScope();
            var addrController = scope?.ServiceProvider?.GetService(typeof(AddressBusinessController)) as AddressBusinessController;
            Assert.That(addrController, Is.Not.Null);

            var input = $"!Comment\nZlatá ulička u Daliborky - sudá č. 28-50";
            var ret = addrController.ProcessText2Output(554782, input);
            Assert.That(ret.Notes.Trim(), Is.Empty);
            Assert.That(ret.Output.Trim(), Is.EqualTo("!Comment\nZlatá ulička u Daliborky - sudá č. 28-50"));
            Assert.That(ret.Input.Trim(), Is.EqualTo(input));
        }
        [Test]
        public void SimpleStreetNumberRuleTest()
        {
            var scope = web?.Services?.CreateScope();
            var addrController = scope?.ServiceProvider?.GetService(typeof(AddressBusinessController)) as AddressBusinessController;
            Assert.That(addrController, Is.Not.Null);

            var input = @$"sudá č.";
            var ret = addrController.GetStreetNumberRules(input);
            var rules = new List<StreetNumberRule>()
            {
                new StreetNumberRule{ From = 0, To = 10000, SeriesType = StreetNumberSeriesType.Even }
            };
            CollectionAssert.AreEqual(rules, ret);
        }
        [Test]
        public void SimpleStreetNumberRuleWithRangeTest()
        {
            var scope = web?.Services?.CreateScope();
            var addrController = scope?.ServiceProvider?.GetService(typeof(AddressBusinessController)) as AddressBusinessController;
            Assert.That(addrController, Is.Not.Null);

            var input = @$"sudá č. 28-50";
            var ret = addrController.GetStreetNumberRules(input);
            var rules = new List<StreetNumberRule>()
            {
                new StreetNumberRule{ From = 28, To = 50, SeriesType = StreetNumberSeriesType.Even }
            };
            CollectionAssert.AreEqual(rules, ret);
        }

        [Test]
        public void OneStreetNumberRuleMultipleRangesTest()
        {
            var scope = web?.Services?.CreateScope();
            var addrController = scope?.ServiceProvider?.GetService(typeof(AddressBusinessController)) as AddressBusinessController;
            Assert.That(addrController, Is.Not.Null);

            var input = @$"č. 28-50, 58, 60 - 62";
            var ret = addrController.GetStreetNumberRules(input);
            var rules = new List<StreetNumberRule>()
            {
                new StreetNumberRule{ From = 28, To = 50, SeriesType = StreetNumberSeriesType.All },
                new StreetNumberRule{ From = 58, To = 58, SeriesType = StreetNumberSeriesType.All },
                new StreetNumberRule{ From = 60, To = 62, SeriesType = StreetNumberSeriesType.All }
            };
            CollectionAssert.AreEqual(rules, ret);
        }

        [Test]
        public void MultipleStreetNumberRuleTest()
        {
            var scope = web?.Services?.CreateScope();
            var addrController = scope?.ServiceProvider?.GetService(typeof(AddressBusinessController)) as AddressBusinessController;
            Assert.That(addrController, Is.Not.Null);

            var input = @$"sudá č. 28-50, lichá č.";
            var ret = addrController.GetStreetNumberRules(input);
            var rules = new List<StreetNumberRule>()
            {
                new StreetNumberRule{ From = 28, To = 50, SeriesType = StreetNumberSeriesType.Even },
                new StreetNumberRule{ SeriesType = StreetNumberSeriesType.Odd }
            };
            CollectionAssert.AreEqual(rules, ret);
        }

        [Test]
        public void MultipleStreetNumberRuleMultipleRangesTest()
        {
            var scope = web?.Services?.CreateScope();
            var addrController = scope?.ServiceProvider?.GetService(typeof(AddressBusinessController)) as AddressBusinessController;
            Assert.That(addrController, Is.Not.Null);

            var input = @$"sudá č. 28-50, 60 - 80, lichá č. 15-17, 21, 25-35, č. p. 255, 608-704, 870";
            var ret = addrController.GetStreetNumberRules(input);
            var rules = new List<StreetNumberRule>()
            {
                new StreetNumberRule{ From = 28, To = 50, SeriesType = StreetNumberSeriesType.Even },
                new StreetNumberRule{ From = 60, To = 80, SeriesType = StreetNumberSeriesType.Even },
                new StreetNumberRule{ From = 15, To = 17, SeriesType = StreetNumberSeriesType.Odd },
                new StreetNumberRule{ From = 21, To = 21, SeriesType = StreetNumberSeriesType.Odd },
                new StreetNumberRule{ From = 25, To = 35, SeriesType = StreetNumberSeriesType.Odd },
                new StreetNumberRule{ From = 255, To = 255, SeriesType = StreetNumberSeriesType.CP },
                new StreetNumberRule{ From = 608, To = 704, SeriesType = StreetNumberSeriesType.CP },
                new StreetNumberRule{ From = 870, To = 870, SeriesType = StreetNumberSeriesType.CP },
            };
            CollectionAssert.AreEqual(rules, ret);
        }

        [Test]
        public void StreetNumberRuleNoUpperBoundRangesTest()
        {
            var scope = web?.Services?.CreateScope();
            var addrController = scope?.ServiceProvider?.GetService(typeof(AddressBusinessController)) as AddressBusinessController;
            Assert.That(addrController, Is.Not.Null);

            var input = @$"sudá č. od 28 výše, lichá č. 7 a výše, č. od 5 a výše, č. p. 700 výše";
            var ret = addrController.GetStreetNumberRules(input);
            var rules = new List<StreetNumberRule>()
            {
                new StreetNumberRule{ From = 28, SeriesType = StreetNumberSeriesType.Even },
                new StreetNumberRule{ From = 7, SeriesType = StreetNumberSeriesType.Odd },
                new StreetNumberRule{ From = 5, SeriesType = StreetNumberSeriesType.All },
                new StreetNumberRule{ From = 700, SeriesType = StreetNumberSeriesType.CP }
            };
            CollectionAssert.AreEqual(rules, ret);
        }
    }
}