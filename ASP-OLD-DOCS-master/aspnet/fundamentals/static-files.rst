.. _fundamentals-static-files:

Working with Static Files
=========================
By `Tom Archer`_

Static files, which include HTML files, CSS files, image files, and JavaScript files, are assets that the app will serve directly to clients. In this article, we'll cover the following topics as they relate to ASP.NET Core and static files.

.. contents:: Sections
  :local:
  :depth: 1

Serving static files
--------------------

By default, static files are stored in the `webroot` of your project. The location of the webroot is defined in the project's ``hosting.json`` file where the default is `wwwroot`.

.. code-block:: none 

  "webroot": "wwwroot"

Static files can be stored in any folder under the webroot and accessed with a relative path to that root. For example, when you create a default Web application project using Visual Studio, there are several folders created within the webroot folder - ``css``, ``images``, and ``js``. In order to directly access an image in the ``images`` subfolder, the URL would look like the following:

  \http://<yourApp>/images/<imageFileName>

In order for static files to be served, you must configure the :doc:`middleware` to add static files to the pipeline. This specific middleware can be configured by adding a dependency on the Microsoft.AspNetCore.StaticFiles package to your project and then calling the ``UseStaticFiles`` extension method from ``Startup.Configure`` as follows:

.. code-block:: c#
  :emphasize-lines: 5

  public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
  {
    ...
    // Add static files to the request pipeline.
    app.UseStaticFiles();
    ...

Now, let's say that you have a project hierarchy where the static files you wish to serve are outside the webroot. For example,let's take a simple layout like the following:

  - wwwroot

    - css
    - images
    - ...

  - MyStaticFiles

    - test.png

In order for the user to access test.png, you can configure the static files middleware as follows:

.. code-block:: c#
  :emphasize-lines: 5-9

  public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
  {
    ...
    // Add MyStaticFiles static files to the request pipeline.
    app.UseStaticFiles(new StaticFileOptions()
    {
        FileProvider = new PhysicalFileProvider(@"D:\Source\WebApplication1\src\WebApplication1\MyStaticFiles"),
        RequestPath = new PathString("/StaticFiles")
    });
    ...

At this point, if the user enters an address of ``http://<yourApp>/StaticFiles/test.png``, the ``test.png`` image will be served.

Enabling directory browsing
---------------------------

Directory browsing allows the user of your Web app to see a list of directories and files within a specified directory (including the root). By default, this functionality is not available such that if the user attempts to display a directory within an ASP.NET Web app, the browser displays an error. To enable directory browsing for your Web app, call the ``UseDirectoryBrowser`` extension method from  ``Startup.Configure`` as follows:

.. code-block:: c#
  :emphasize-lines: 5

  public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
  {
    ...
    // Turn on directory browsing for the current directory.
    app.UseDirectoryBrowser();
    ...

The following figure illustrates the results of browsing to the Web app's ``images`` folder with directory browsing turned on:

.. image:: static-files/_static/dir-browse.png

Now, let's say that you have a project hierarchy where you want the user to be able to browse a directory that is not in the webroot. For example, let's take a simple layout like the following:

  - wwwroot

    - css
    - images
    - ...

  - MyStaticFiles

In order for the user to browse the ``MyStaticFiles`` directory, you can configure the static files middleware as follows:

.. code-block:: c#
  :emphasize-lines: 5-9

  public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
  {
    ...
    // Add the ability for the user to browse the MyStaticFiles directory.
    app.UseDirectoryBrowser(new DirectoryBrowserOptions()
    {
        FileProvider = new PhysicalFileProvider(@"D:\Source\WebApplication1\src\WebApplication1\MyStaticFiles"),
        RequestPath = new PathString("/StaticFiles")
    });
    ...

At this point, if the user enters an address of ``http://<yourApp>/StaticFiles``, the browser will display the files in the ``MyStaticFiles`` directory.

Serving a default document
--------------------------

Setting a default home page gives site visitors a place to start when visiting your site. Without a default site users will see a blank page unless they enter a fully qualified URI to a document.  In order for your Web app to serve a default page without the user having to fully qualify the URI, call the ``UseDefaultFiles`` extension method from ``Startup.Configure`` as follows.

.. code-block:: c#
  :emphasize-lines: 5-6

  public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
  {
    ...
    // Serve the default file, if present.
    app.UseDefaultFiles();
    app.UseStaticFiles();
    ...

.. note:: ``UseDefaultFiles`` must be called before ``UseStaticFiles`` or it will not serve up the default home page. You must still call ``UseStaticFiles``. ``UseDefaultFiles`` is a URL re-writer that doesn't actually serve the file. You must still specify middleware (UseStaticFiles, in this case) to serve the file.

If you call the ``UseDefaultFiles`` extension method and the user enters a URI of a folder, the middleware will search (in order) for one of the following files. If one of these files is found, that file will be used as if the user had entered the fully qualified URI (although the browser URL will continue to show the URI entered by the user).

  - default.htm
  - default.html
  - index.htm
  - index.html

To specify a different default file from the ones listed above, instantiate a ``DefaultFilesOptions`` object and set its ``DefaultFileNames`` string list to a list of names appropriate for your app. Then, call one of the overloaded ``UseDefaultFiles`` methods passing it the ``DefaultFilesOptions`` object. The following example code removes all of the default files from the ``DefaultFileNames`` list and adds  ``mydefault.html`` as the only default file for which to search.

.. code-block:: c#
  :emphasize-lines: 5-9

  public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
  {
    ...
    // Serve my app-specific default file, if present.
    DefaultFilesOptions options = new DefaultFilesOptions();
    options.DefaultFileNames.Clear();
    options.DefaultFileNames.Add("mydefault.html");
    app.UseDefaultFiles(options);
    app.UseStaticFiles();
    ...

Now, if the user browses to a directory in the webroot with a file named ``mydefault.html``, that file will be served as though the user typed in the fully qualified URI.

But, what if you want to serve a default page from a directory that is outside the webroot directory? You could call both the ``UseStaticFiles`` and ``UseDefaultFiles`` methods passing in identical values for each method's parameters. However, it's much more convenient and recommended to call the ``UseFileServer`` method, which is covered in the next section.

Using the UseFileServer method
------------------------------

In addition to the ``UseStaticFiles``, ``UseDefaultFiles``, and ``UseDirectoryBrowser`` extensions methods, there is also a single method - ``UseFileServer`` - that combines the functionality of all three methods. The following example code shows some common ways to use this method:

.. code-block:: c#

  // Enable all static file middleware (serving of static files and default files) EXCEPT directory browsing.
  app.UseFileServer();

.. code-block:: c#

  // Enables all static file middleware (serving of static files, default files, and directory browsing).
  app.UseFileServer(enableDirectoryBrowsing: true);

As with the ``UseStaticFiles``, ``UseDefaultFiles``, and ``UseDirectoryBrowser`` methods, if you wish to serve files that exist outside the webroot, you instantiate and configure an "options" object that you pass as a parameter to ``UseFileServer``. For example, let's say you have the following directory hierarchy in your Web app:

- wwwroot

  - css
  - images
  - ...

- MyStaticFiles

  - test.png
  - default.html

Using the hierarchy example above, you might want to enable static files, default files, and browsing for the ``MyStaticFiles`` directory. In the following code snippet, that is accomplished with a single call to ``UseFileServer``.

.. code-block:: c#

  // Enable all static file middleware (serving of static files, default files,
  // and directory browsing) for the MyStaticFiles directory.
  app.UseFileServer(new FileServerOptions()
  {
      FileProvider = new PhysicalFileProvider(@"D:\Source\WebApplication1\src\WebApplication1\MyStaticFiles"),
      RequestPath = new PathString("/StaticFiles"),
      EnableDirectoryBrowsing = true
  });

Using the example hierarchy and code snippet from above, here's what happens if the user browses to various URIs:

  - ``http://<yourApp>/StaticFiles/test.png`` - The ``MyStaticFiles/test.png`` file will be served to and presented by the browser.
  - ``http://<yourApp>/StaticFiles`` - Since a default file is present (``MyStaticFiles/default.html``), that file will be served. If that file didn't exist, the browser would present a list of files in the ``MyStaticFiles`` directory (because the ``FileServerOptions.EnableDirectoryBrowsing`` property is set to ``true``).

Working with content types
--------------------------

The ASP.NET static files middleware understands almost 400 known file content types. If the user attempts to reach a file of an unknown file type, the static file middleware will not attempt to serve the file.

Let's take the following directory/file hierarchy example to illustrate:

- wwwroot

  - css
  - images

    - test.image

  - ...

Using this hierarchy, you could enable static file serving and directory browsing with the following:

.. code-block:: c#
  :emphasize-lines: 5-6

  public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
  {
    ...
    // Serve static files and allow directory browsing.
    app.UseDirectoryBrowser();
    app.UseStaticFiles();

If the user browses to ``http://<yourApp>/images``, a directory listing will be displayed by the browser that includes the ``test.image`` file. However, if the user clicks on that file, they will see a 404 error - even though the file obviously exists. In order to allow the serving of unknown file types, you could set the ``StaticFileOptions.ServeUnknownFileTypes`` property to ``true`` and specify a default content type via ``StaticFileOptions.DefaultContentType``. (Refer to this `list of common MIME content types <http://www.freeformatter.com/mime-types-list.html>`_.)

.. code-block:: c#
  :emphasize-lines: 5-10

  public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
  {
    ...
    // Serve static files and allow directory browsing.
    app.UseDirectoryBrowser();
    app.UseStaticFiles(new StaticFileOptions
    {
      ServeUnknownFileTypes = true,
      DefaultContentType = "image/png"
    });

At this point, if the user browses to a file whose content type is unknown, the browser will treat it as an image and render it accordingly.

So far, you've seen how to specify a default content type for any file type that ASP.NET doesn't recognize. However, what if you have multiple file types that are unknown to ASP.NET? That's where the ``FileExtensionContentTypeProvider`` class comes in.

The ``FileExtensionContentTypeProvider`` class contains an internal collection that maps file extensions to MIME content types. To specify custom content types, simply instantiate a ``FileExtensionContentTypeProvider`` object and add a mapping to the ``FileExtensionContentTypeProvider.Mappings`` dictionary for each needed file extension/content type. In the following example, the code adds a mapping of the file extension ``.myapp`` to the MIME content type ``application/x-msdownload``.

.. code-block:: c#
  :emphasize-lines: 5-13

  public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
  {
    ...

    // Allow directory browsing.
    app.UseDirectoryBrowser();

    // Set up custom content types - associating file extension to MIME type
    var provider = new FileExtensionContentTypeProvider();
    provider.Mappings.Add(".myapp", "application/x-msdownload");

    // Serve static files.
    app.UseStaticFiles(new StaticFileOptions { ContentTypeProvider = provider });

    ...

Now, if the user attempts to browse to any file with an extension of ``.myapp``, the user will be prompted to download the file (or it will happen automatically depending on the browser).

IIS Considerations
------------------

ASP.NET Core applications hosted in IIS use the HTTP platform handler to forward all requests to the application including requests for static files. The IIS static file handler is not used because it won???t get a chance to handle the request before it is handled by the HTTP platform handler.

Best practices
--------------

This section includes a list of best practices for working with static files:

  - Code files (including C# and Razor files) should be placed outside of the app project's webroot. This creates a clean separation between your app's static (non-compilable) content and source code.

Summary
-------
In this article, you learned how the static files middleware component in ASP.NET Core allows you to serve static files, enable directory browsing, and serve default files. You also saw how to work with content types that ASP.NET doesn't recognize. Finally, the article explained some IIS considerations and presented some best practices for working with static files.

Additional Resources
--------------------

- :doc:`middleware`
