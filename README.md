## App.config

```xml
<appender name="FlockAppender" type="Log4Flock.FlockAppender, Log4Flock">
  <WebhookUrl value="https://api.flock.com/hooks/sendMessage/TOKENHERE!!!" />  <!-- Your Flock webhook URL -->
  <AddAttachment value="true" /> <!-- Include an attachment on the Flock message with additional details -->
  <AddExceptionTraceField value="true" /> <!-- If an exception occurs, add its trace as a field on the message attachment -->
  <OpenStackTraceInModal value="true" /> <!-- Includes a button for the attachment to see stacktrace, if false it will be included in message -->
  <DownloadStackTrace value="true" /> <!-- Includes a button to download the stacktrace -->
  <StackTraceModalUrl value="https://STACKTRACEURLHERE!!!" /> <!-- Url for the stacktrace viewer -->
  <UsernameAppendLoggerName value="true"/> <!-- Append the current logger name to the message -->
  <Proxy value="http://proxy:8000"/> <!-- Use an outgoing http proxy -->
  <layout type="log4net.Layout.PatternLayout">
    <conversionPattern value="%message" />
  </layout>
</appender>
```

## Authors

* **Rasmus Jørgensen** - *Customized it to work with Flock*
* **John Freeland** - *Initial work for Log4Slack* - [Log4Slack](https://github.com/jonfreeland/Log4Slack)

See also the list of [contributors](https://github.com/smaakage/Log4Flock/graphs/contributors) who participated in this project.

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details

[Github Link](https://github.com/smaakage/Log4Flock/)
