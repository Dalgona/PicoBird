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
  
  HttpResponseMessage res = await api.Get(
      "/statuses/home_timeline.json",
      new NameValueCollection
      {
          { "include_entities", "true" }
      });
  ```
  
## Todo

* request_token
* Add basic wrappers

## License

*TBD*

## Copyright

Copyright &copy; 2016, Dalgona. <dalgona@hontou.moe>
