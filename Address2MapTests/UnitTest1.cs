using Address2Map.BusinessController;
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
            Assert.That(ret.Count(), Is.EqualTo(27));
        }
#if REGEXWorks
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
#endif
    }
}