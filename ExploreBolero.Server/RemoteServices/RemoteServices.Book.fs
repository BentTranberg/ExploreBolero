namespace ExploreBolero.Server.RemoteServices

open System.IO
open Microsoft.AspNetCore.Hosting
open Bolero
open Bolero.Remoting.Server
open ExploreBolero.Client

// This remote service requires injected dependencies (namely, `env`),
// so it must be defined as a RemoteHandler class.
type BookService(ctx: IRemoteContext, env: IWebHostEnvironment) =
    inherit RemoteHandler<Books.RemoteService>()

    let books =
        Path.Combine(env.ContentRootPath, "data/books.json")
        |> File.ReadAllText
        |> Json.Deserialize<Books.Book[]>
        |> ResizeArray

    override this.Handler =
        {
            getBooks = ctx.Authorize <| fun () -> async {
                return books.ToArray()
            }

            addBook = ctx.Authorize <| fun book -> async {
                books.Add(book)
            }

            removeBookByIsbn = ctx.Authorize <| fun isbn -> async {
                books.RemoveAll(fun b -> b.isbn = isbn) |> ignore
            }
        }
