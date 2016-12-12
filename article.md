### C# lambda, SQS, SNS and Cloudwatch - together for the first time
Created by Andrei Gec, last modified on Dec 09, 2016

The next logical step after breaking up a monolithic app/database into microservices is a serverless approach.  Here, instead of depending on traditional EC2 instances to host and run services in the traditional fashion, we utilise FAAS (function as a service)<sup>[[1]](https://en.wikipedia.org/wiki/Function_as_a_Service)</sup> patterns to run functions on demand.

The benefits of a serverless approach:

*   Reduced computational overhead - no need to run a service 24x7, just run a function when computation is required.
*   Cost - the cost reduction from running virtual computers can be upwards of 90%, and scaling functions horizontally is automatic.<sup>[[2]](http://martinfowler.com/articles/serverless.html#Faas-ScalingCosts)</sup>
*   Reduced complexity/code - a lot of the normal middleware for running APIs and websites can be eliminated or reduced - for example instead of running a normal api, we can combine api gateway and lambda, and have most of the "ilities"<sup>[[3]](https://en.wikipedia.org/wiki/Non-functional_requirement)</sup> covered by default.<sup><span style="color: rgb(0,51,102);">[<span style="color: rgb(0,51,102);">[4]</span>](https://www.iron.io/what-is-serverless-computing/)</span></sup>

In addition there are a few downsides:

*   Infrequently used functions can spin down, leading to start up times in the tens of seconds - as long as the function is run periodically, this shouldn't be an issue.
*   Resource limits - time & memory. Each function's max memory allowance must be specified in advance, and functions have a hard limit on execution time - in the order of several minutes, so longer running tasks are not suitable.
*   Potential for abuse. There are no limits on the number of concurrent functions that can be run, so a poorly triggered function can be run far more often than was intended.

Given the recent announcement of C# dotnet core being natively supported in aws lambda<sup>[[5]](https://aws.amazon.com/blogs/compute/announcing-c-sharp-support-for-aws-lambda/)</sup>, lets dive in and create a proof of concept lambda function that consumes items off an SQS queue:

### Overview:

We will use cloudwatch to listen to the number of items in an SQS queue, and trigger an SNS notification, which will trigger our lambda function.

SQS: aws simple queuing

SNS: aws notifications - broadcasts messages to subscribers, including but not limited to lambda functions and email addresses

Cloudwatch: triggers subscribers either periodically, or by checking single metrics such as number of messages in a queue.

![alt text](https://raw.githubusercontent.com/andreigec/AWSCSharpLambda/master/images/image2016-12-9%2016-55-28.png)

## Step 1: Create IAM users

*   Make a user to consume messages off an SQS queue.  Note: for this demo, an overly permissive policy will be used.

    **make sure you save the credentials**

![alt text](https://raw.githubusercontent.com/andreigec/AWSCSharpLambda/master/images/image2016-12-9%2012-56-9.png)

![alt text](https://raw.githubusercontent.com/andreigec/AWSCSharpLambda/master/images/image2016-12-9%2012-56-33.png)

## Step 2: Creating a SQS queue

*   Create a queue with the default settings:  

![alt text](https://raw.githubusercontent.com/andreigec/AWSCSharpLambda/master/images/image2016-12-9%2010-32-21.png)

## Step 3: Creating the C# lambda function.

Creating a C# lambda function is very straightforward with 2015.3, and the vs aws toolkit<sup>[[6]](https://aws.amazon.com/sdk-for-net/)</sup>, having its own template:

![alt text](https://raw.githubusercontent.com/andreigec/AWSCSharpLambda/master/images/image2016-12-9%2010-27-57.png)

Packages to install: AWSSDK.SQS >= 3.3.1.2

*   The default entry point has an instance of ILambdaContext, so far I've only needed this to attach logging.
*   The aws sdk for SQS is pretty simple to use:
    *   Provide basic credentials
    *   Create a connection to the AZ we are using (sydney)
    *   Receive a message from a queue which we pass in as a string
    *   Accept the message and delete it by reusing the receipt handle from the read

Full code can be found here:

[https://github.com/andreigec/AWSCSharpLambda](https://github.com/andreigec/AWSCSharpLambda)

*   To deploy initially, go to the folder the lambda project file is in and run **dotnet lambda deploy-function**
*   you will need to specify the c# function name, as well as an IAM used to deploy. For this demo, I created an IAM role in the console to deploy:

![alt text](https://raw.githubusercontent.com/andreigec/AWSCSharpLambda/master/images/image2016-12-9%2013-17-31.png)

Now we can find the function in lambda:

![alt text](https://raw.githubusercontent.com/andreigec/AWSCSharpLambda/master/images/image2016-12-9%2013-18-27.png)

**To run the function, I've changed the example text parameter to be a simple string - passing in json can cause errors**

![alt text](https://raw.githubusercontent.com/andreigec/AWSCSharpLambda/master/images/image2016-12-9%2013-19-30.png)

The example output is as follows:

`
START RequestId: f0273470-bdb5-11e6-9889-09db0b612fb0 Version: $LATEST
[2:19:39 AM +00]csharp test function-Info Hello
END RequestId: f0273470-bdb5-11e6-9889-09db0b612fb0
REPORT RequestId: f0273470-bdb5-11e6-9889-09db0b612fb0  Duration: 0.97 ms   Billed Duration: 100 ms     Memory Size: 128 MB Max Memory Used: 47 MB
`

At this point I've added in the code to read from SQS into C# and redeployed.

## Step 4: Creating an SNS topic

*   Create an SNS topic. You will need to use a truncated name for the "display name" field.

![alt text](https://raw.githubusercontent.com/andreigec/AWSCSharpLambda/master/images/image2016-12-9%2010-35-8.png)

<div>

*   Copy the ARN of the new topic
*   Create a subscription and link to lambda  

</div>

![alt text](https://raw.githubusercontent.com/andreigec/AWSCSharpLambda/master/images/image2016-12-9%2015-57-47.png)

## Step 5: Hook up cloudwatch to SNS

*   Create a cloudwatch alarm to trigger a message on SNS when there are items in the queue:  

![alt text](https://raw.githubusercontent.com/andreigec/AWSCSharpLambda/master/images/image2016-12-9%2013-24-48.png)

![alt text](https://raw.githubusercontent.com/andreigec/AWSCSharpLambda/master/images/image2016-12-9%2013-25-17.png)

The alarm should be in the OK status.

![alt text](https://raw.githubusercontent.com/andreigec/AWSCSharpLambda/master/images/image2016-12-9%2013-26-2.png)

Adding an item to SQS should trigger the alarm:

![alt text](https://raw.githubusercontent.com/andreigec/AWSCSharpLambda/master/images/image2016-12-9%2013-26-22.png)

![alt text](https://raw.githubusercontent.com/andreigec/AWSCSharpLambda/master/images/image2016-12-9%2013-27-48.png)

It could take up to 5 minutes to change depending on your Cloudwatch metric values:

![alt text](https://raw.githubusercontent.com/andreigec/AWSCSharpLambda/master/images/image2016-12-9%2015-56-42.png)

Now, adding a message to SQS will trigger a message on SNS and trigger lambda, which can be debugged in Cloudwatch/logs

![alt text](https://raw.githubusercontent.com/andreigec/AWSCSharpLambda/master/images/image2016-12-9%2016-40-0.png)

![alt text](https://raw.githubusercontent.com/andreigec/AWSCSharpLambda/master/images/image2016-12-9%2016-41-4.png)

## Addendum

*   Cloudwatch alarms do not trigger their action more than once. If you have a lambda task that should be run several times until the queue is empty, one way I've found to fix this is to use the Cloudwatch API in csharp to programatically reset the Cloudwatch alarm to the "OK" status. This way, the alarm will get triggered again, but having an option to periodically fire an event once the alarm has triggered would be a nice addition to Cloudwatch.
