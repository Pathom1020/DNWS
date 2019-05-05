using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using System.Linq;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace DNWS
{
    class TwitterApiPlugin:TwitterPlugin
    {
        private List<User> GetUser()
        {
            using (var context = new TweetContext())
            {
                try
                {
                    List<User> users = context.Users.Where(b => true).Include(b => b.Following).ToList();
                    return users;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public List<Following> GetFollowing(string name)
        {
            using (var context = new TweetContext())
            {
                try
                {
                    List<User> followings = context.Users.Where(b => b.Name.Equals(name)).Include(b => b.Following).ToList();
                    return followings[0].Following;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public override HTTPResponse GetResponse(HTTPRequest request)
        {
            HTTPResponse response = new HTTPResponse(200);

            string user = request.getRequestByKey("user");
            string password = request.getRequestByKey("password");
            string following = request.getRequestByKey("following");
            string fllw_timeline = request.getRequestByKey("timeline");
            string text = request.getRequestByKey("text");
            string msg = request.getRequestByKey("message");
            string[] path = request.Filename.Split("?");
            try
            {
                if (path[0] == "name")
                {
                    if (request.Method == "GET")
                    {
                        string js = JsonConvert.SerializeObject(GetUser());
                        response.body = Encoding.UTF8.GetBytes(js);
                    }
                    else if (request.Method == "POST")
                    {
                        if (user != null && password != null)
                        {
                            Twitter.AddUser(user, password);
                        }
                    }
                    else if (request.Method == "DELETE")
                    {
                        if (user != null)
                        {
                            Twitter.RemoveUser(user);
                        }
                    }
                }
                else if (path[0] == "following")
                {
                    Twitter twitter = new Twitter(user);
                    if (request.Method == "GET")
                    {
                        string temp = JsonConvert.SerializeObject(GetFollowing(user));
                        response.body = Encoding.UTF8.GetBytes(temp);
                    }
                    if (request.Method == "POST")
                    {
                        Twitter follow = new Twitter(user);
                        follow.AddFollowing(following);
                        response.body = Encoding.UTF8.GetBytes("Success Following");
                    }
                }
                else if (path[0] == "tweets")
                {
                    Twitter twitter = new Twitter(user);
                    if (request.Method == "GET")
                    {
                        if (fllw_timeline == "user")
                        {
                            string temp = JsonConvert.SerializeObject(twitter.GetUserTimeline());
                            response.body = Encoding.UTF8.GetBytes(temp);
                        }
                        if (fllw_timeline == "follow")
                        {
                            string temp = JsonConvert.SerializeObject(twitter.GetFollowingTimeline());
                            response.body = Encoding.UTF8.GetBytes(temp);
                        }
                    }
                    if (request.Method == "POST")
                    {
                        twitter.PostTweet(text);
                        response.body = Encoding.UTF8.GetBytes("Post Success");
                    }

                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                Console.WriteLine(ex.ToString());
                sb.Append(String.Format("Error [{0}], please go back to <a href=\"/twitter\">login page</a> to try again", ex.Message));
                response.body = Encoding.UTF8.GetBytes(sb.ToString());
                return response;
            }
            response.type = "application/json; charset=UTF-8";
            return response;
        }

    }
}
