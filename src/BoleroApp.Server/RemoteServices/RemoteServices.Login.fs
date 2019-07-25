namespace BoleroApp.Server.RemoteServices

open System
open Bolero.Remoting.Server
open BoleroApp.Client

module LoginService =

    // This remote service doesn't require any injected dependencies,
    // so it can be defined as a top-level value.
    let service : Login.RemoteService =
        {
            signIn = Remote.withContext <| fun http (username, password) -> async {
                if password = "password" then
                    do! http.AsyncSignIn(username, TimeSpan.FromDays(365.))
                    return Some username
                else
                    return None
            }

            signOut = Remote.withContext <| fun http () -> async {
                return! http.AsyncSignOut()
            }

            getUsername = Remote.authorize <| fun http () -> async {
                return http.User.Identity.Name
            }
        }
