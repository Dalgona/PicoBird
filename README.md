# PicoBird
Simple Twitter library for .NET

## Usage

* Initialize

  ```cs
  PicoBird.API api = new PicoBird.API("<CONSUMER_KEY>", "<CONSUMER_SECRET>");
  api.Token = "<OAUTH_TOKEN>";
  api.TokenSecret = "<OAUTH_TOKEN_SECRET>";
  ```

* Access Twitter API Resources

  ```cs
  using System.Collections.Specialized;
  using System.Net.Http;
  
  // PicoBird.API.Get() method returns raw HttpResponseMessage objects.
  HttpResponseMessage res = await api.Get(
      "/1.1/statuses/home_timeline.json",
      new NameValueCollection
      {
          { "include_entities", "true" }
      });
  ```
  
* Using Twitter object wrapper classes (defined in `PicoBird.Objects` namespace)

  ```cs
  using PicoBird.Objects;
  
  User user = await api.Get<User>("/1.1/account/verify_credentials.json");
  Console.WriteLine($"Name: {user.name}");
  
  Tweet[] home = await api.Get<Tweet[]>("/1.1/statuses/home_timeline.json");
  foreach (Tweet status in home)
      Console.WriteLine($"@{status.user.screen_name}: {status.text}");
  ```

## Todo

* request_token
* Add basic wrappers

## License

*TBD*

## Credits

This project uses Newtonsoft's [Json.NET](http://www.newtonsoft.com/json) ([MIT License](https://github.com/JamesNK/Newtonsoft.Json/blob/master/LICENSE.md))

## Copyright

Copyright &copy; 2016, Dalgona. <dalgona@hontou.moe>
