AvayaCPaaS
===========

This .net library is an open source tool built to simplify interaction with the [Avaya CPaaS](https://www.avaya.com/en/products/cloud/cpaas/) telephony platform. Avaya CPaaS makes adding voice and SMS to applications fun and easy.

For more information about Avaya CPaaS, please visit: [Avaya OneCloud™️ CPaaS ](https://www.avaya.com/en/products/cloud/cpaas/)

To read the official documentation, please visit: [Avaya CPaaS Docs](https://docs.avayacloud.com/aspx/docs)

---


Installation
============

##### Using Visual Studio and NuGet Package Manager

- In Solution Explorer, right click on References and choose Manage NuGet Packages.
- Choose nuget.org as the Package source, click the Browse tab, search for AvayaCPaaS, select that package in the list, and click Install
- Right-click the solution in Solution Explorer and click Build Solution

Usage
======

### REST

See the [Avaya CPaaS REST API documentation](https://docs.avayacloud.com/aspx/rest) for more information.

#### Make call Example

```cs
using System;
using AvayaCPaaS.Configuration;
using AvayaCPaaS.Exceptions;

public class Program
    {
        static void Main(string[] args)
        {
            var configuration = new APIConfiguration({AccountSid}, {AuthToken});
			var service = new CPaaSService(configuration);

		public void MakeCall()
        {
            try
            {
                // Make call using calls connector
                var call = service.CallsConnector.MakeCall("+12345", "+12678", "http://static.zang.io/ivr/welcome/call.xml", playDtmf: "ww12w3221", timeout: 100);
                Console.WriteLine(call.Status);
            }
            catch (CPaaSException e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
```


#### Configuration (3 ways)

First, a configuration must be created by using provided configuration class `APIConfiguration` or by creating your own implementation of `IAPIConfiguration` interface.

##### 1. Create APIConfiguration with parameters AccountSid and AuthToken

Normally you'll want to just enter your Avaya CPaaS Platform *AccountSid* and *AuthToken*, but you can also define a proxy server or change the base API URL.
Basic configuration:

```cs
var configuration = new APIConfiguration({AccountSid}, {AuthToken});
```
or if you want to define a proxy server:
```cs
var configuration = new APIConfiguration({AccountSid}, {AuthToken});
configuration.UseProxy = true;
configuration.ProxyHost = {ProxyHost}
configuration.ProxyPort = {ProxyPort}
```
Next, you'll have to create a CPaaSService and pass the configuration
```cs
var service = new CPaaSService(configuration)
```

##### 2. Use a custom HttpManager
Another way to create CPaaSService is to pass a custom HttpManager which implements IHttpManager. 
```cs
var service = new CPaaSService(myHttpManager)
```

##### 3. Use configuration parameters from `app.config` file

Create CPaaSService with empty APIConfiguration
```cs
var service = new CPaaSService(new APIConfiguration("", ""));
```

Configure service 
```cs
service.InitFromConfig();
```


#### Request parameters
Request parameters are passed as parameters to connector object methods as shown previously in *Send SMS Example*. All methods have convenience overloads which use the `AccountSid` parameter specified in the configuration automatically.

Explicit accountSid
```cs
var usage = service.UsagesConnector.ViewUsage({accountSid}, {UsageSid});
```

AccountSid from configuration used automatically
```cs
var usage = service.UsagesConnector.ViewUsage({UsageSid});
```

##### Methods with optional parameters

Optional parameters are used as named parameters like `transcribe:true`, e.g:
```cs
var call = service.CallsConnector.MakeCall("+12345", "+112233", "testUrl", transcribe:true, transcribeCallback:"transcribeCallback");
```

#### Response data
The received data can be an object, e.g.:

```cs
var usage = service.UsagesConnector.ViewUsage({UsageSid});
Console.WriteLine(usage.TotalCost);
```
Or a list of objects in which case the list is iterable, e.g.:
```cs
var usages = service.UsagesConnector.ListUsages(year:2017, month:5, product:Product.INBOUND_CALL, page: 3, pageSize: 40);

foreach (var usage in usages.Elements)
{
      Console.WriteLine(usage.TotalCost);              
}
```

### InboundXML

InboundXML is an XML dialect which enables you to control phone call flow. For more information please visit the [Avaya CPaaS InboundXML documentation](http://docs.zang.io/aspx/inboundxml).

Example of using InboundXML builder:

```cs
using System;
using AvayaCPaaS.InboundXml;
using AvayaCPaaS.Exceptions;

public class Program
{
    static void Main(string[] args)
    {
    	try
        {
            var builder = new InboundXmlBuilder();
     
            builder.GetRequestNode()
		        .Dial("(555)555-5555", hideCallerId:false, dialMusic:"dial music", confirmSound:"dial confirm sound",                              digitsMatch:"ww12w3221", record:false, recordDirection:RecordDirectionEnum.@out)
		            .StartInner()
		            .Sip("username@domain.com", "username", "password")
		            .EndInner()
		        .Gather()
		            .StartInner()
		            .Say(language: LanguageEnum.en, loop: 3, value: "Welcome to Avaya CPaaS!", voice: VoiceEnum.female)
		            .Pause(length: 2)
		            .EndInner()
		        .Hangup(schedule: 4, reason: HangupReasonEnum.rejected);

            var result = builder.Build();
                           
            Console.WriteLine(result);
        }
        catch (CPaaSException e)
        {
            Console.WriteLine(e.Message);
        }
    }
}
```

will render

```xml
<Response>
  <Dial dialMusic="dial music" confirmSound="dial confirm sound" digitsMatch="ww12w3221" hideCallerId="False" record="False" recordDirection="out">
    <Sip username="username" password="password">username@domain.com</Sip>
  </Dial>
  <Gather>
    <Say loop="3" voice="female" language="en">Welcome to Avaya CPaaS!</Say>
    <Pause length="2" />
  </Gather>
  <Hangup schedule="4" reason="rejected" />
</Response>
```

### Log Exception

Exception can be log into file.
Define your directory path value into variable `CPAAS_LOG_PATH` which is located in the cpaas-dotnet/ZangAPI.Examples/App.config