# Tapestry Notifications

A serverless event driven push notification system which traverses tapestryjournal.com, finds new observations, or comments and sends a push notification to the recipient (via mobile [Prowl](https://www.prowlapp.com) app).

## Set up

To use this yourself, you'll need to set up some environment variables:

* `"PROWL_API_KEY"` - your prowl mobile app API Key
* `"TAPESTRY_URL"` - <https://tapestryjournal.com/>
* `"TAPESTRY_EMAIL"` - your tapestry login
* `"TAPESTRY_PASSWORD"` - your tapestry password

## The Problem

### 2021: National Lockdown occurs

In the start of 2021, schools closed and a UK national lockdown was put in place. People were forced to homeschool their children and yet work from home at the same time. This is the impossible position both I and my wife found ourselves in.

## TapstryJournal.com website

The school my son goes to in the UK found that they could use their online resource <http://tapestryjournal.com> *(which was used originally just for school pupil observations)* to do just about anything they could with, to support home schooling. Something the website *(nor their push-notification-poor mobile app)* wasn't designed for. I would be working during the day and either completely miss responses, or new assignments, OR would have to sit in an evening and go through the site manually. There had to be a better option.

## The Technology

I work with Azure, and love both PaaS and serverless. When it comes to Azure functions/Durable Functions/Durable Entities I'm all in *(my love also extends to 2020-newbies: GitHub actions and Azure Static Web Apps)*.

Using a one-time price-of-a-coffee payment to <http://prowlapp.com> *(which I bought decades ago now)* I can send myself near unlimited push notifications, programatically.

Prowl may be old, but it does 2 things newer offerings do not: a ridiculous rate limit of 1000 calls/hour, *(I mean! who even wants to send themselves a push notification to their phone every 3.6 seconds?)* but also a great 1000-character limit in the Push description field.

I knew from my other home-experiments that I could combine:

* An Azure Timer Function
* Durable Entities
* Puppeteer
* Headless Chrome
* [Prowl](https://www.prowlapp.com)
* Dependency Injection
* [C# 8 Nullable value types](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/nullable-value-types)
* GitHub Actions

... to log in, traverse, memorise, analyse, then send me notifications of new entries I might miss.

## The Solution

So if you've not guessed by now, every hour, a Timer Azure Function wakes up. It... 

* downloads headless chrome
  * *(kudos to the ... and I mean this when I say this ... [sick blog post by Azure Function PM hero Anthony Chu](https://anthonychu.ca/post/azure-functions-puppeteer-pdf-razor-template/) which demonstrates headless chrome in Azure Functions)*
* logs into tapestry, 
* finds all the observation titles, ids, urls, 
* goes through each one, then 
* sends what it finds to a processing routine. 
* This routine either sends via push-notification new entries to me
  * then saves to a Durable Entity if new, 
* or, if a durable entity already exists, compares it's observation comments and notifies me if the latest one has changed.

## Conclusion

This whole repo took me a couple of nights to set up *(when else do I have the time? 😆)* but in effect, I can now work, homeschool AND have my phone pop up important push notifications as I go through my day. This frees my evenings to relax, rather than stressing about the next day - as we all have enough to think about now a days.