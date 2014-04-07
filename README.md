## This is an add-in for [Fody](https://github.com/Fody/Fody/) 

Compile time decorator pattern via IL rewriting.

[Introduction to Fody](http://github.com/Fody/Fody/wiki/SampleUsage)

This version is fork of [Fody/MethodDecorator](https://github.com/Fody/MethodDecorator) with changes I found useful

Differences from original Fody/MethodDecorator:
* No attributes or interfaces in root namespace (actually without namespace) are required
* Interceptor attribute can be declared and implemented in a separate assembly
* Init method is called before any method and receives the method reference and args 
* OnEntry/OnExit/OnException methods don't receive the method reference anymore
* IntersectMethodsMarkedByAttribute attribute allows you to intersect a method marked by any attribute

### Your Code
	// Atribute should be "registered" by adding as module or assembly custom attribute
	[module: Interceptor]
	
	// Any attribute which provides OnEntry/OnExit/OnException with proper args
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Assembly | AttributeTargets.Module)]
	public class InterceptorAttribute : Attribute, IMethodDecorator	{
	    // instance, method and args can be captured here and stored in attribute instance fields
		// for future usage in OnEntry/OnExit/OnException
		public void Init(object instance, MethodBase method, object[] args) {
			TestMessages.Record(string.Format("Init: {0} [{1}]", method.DeclaringType.FullName + "." + method.Name, args.Length));
		}
		public void OnEntry() {
	        TestMessages.Record("OnEntry");
	    }
	
	    public void OnExit() {
	        TestMessages.Record("OnExit");
	    }
	
	    public void OnException(Exception exception) {
	        TestMessages.Record(string.Format("OnException: {0}: {1}", exception.GetType(), exception.Message));
	    }
	}
	
	public class Sample	{
		[Interceptor]
		public void Method()
		{
		    Debug.WriteLine("Your Code");
		}
	}

### What's gets compiled
	
	public class Sample {
		public void Method(int value) {
		    InterceptorAttribute attribute = 
		        (InterceptorAttribute) Activator.CreateInstance(typeof(InterceptorAttribute));
		    
			// in c# __methodref and __typeref don't exist, but you can create such IL 
			MethodBase method = MethodBase.GetMethodFromHandle(__methodref (Sample.Method), 
															   __typeref (Sample));
		    
			object[] args = new object[1] { (object) value };
			
			attribute.Init((object)this, method, args);

			attribute.OnEntry();
		    try {
		        Debug.WriteLine("Your Code");
		        attribute.OnExit();
		    }
		    catch (Exception exception) {
		        attribute.OnException(exception);
		        throw;
		    }
		}
	}

**NOTE:** *this* is replaced by *null* when the decorated method is static or a constructor.

### IntersectMethodsMarkedByAttribute

This is supposed to be used as	

	// all MSTest methods will be intersected by the code from IntersectMethodsMarkedBy 
	[module:IntersectMethodsMarkedBy(typeof(TestMethod))] 

You can pass as many marker attributes to IntersectMethodsMarkedBy as you want
	
	[module:IntersectMethodsMarkedBy(typeof(TestMethod), typeof(Fact), typeof(Obsolete))]

Example of IntersectMethodsMarkedByAttribute implementation

	[AttributeUsage(AttributeTargets.Module | AttributeTargets.Assembly)]
	public class IntersectMethodsMarkedByAttribute : Attribute {
		// Required
		public IntersectMethodsMarkedByAttribute() {}

		public IntersectMethodsMarkedByAttribute(params Type[] types) {
			if (types.All(x => typeof(Attribute).IsAssignableFrom(x))) {
				throw new Exception("Meaningfull configuration exception");
			}
		}
		public void Init(object instance, MethodBase method, object[] args) {}
		public void OnEntry() {}
		public void OnExit() {}
		public void OnException(Exception exception) {}
	}

Now all your code marked by [TestMethodAttribute] will be intersected by IntersectMethodsMarkedByAttribute methods.
You can have multiple IntersectMethodsMarkedByAttributes applied if you want (don't have idea why). 
MethodDecorator searches IntersectMethodsMarkedByAttribute by predicate StartsWith("IntersectMethodsMarkedByAttribute")

### How to get it

NuGet: https://www.nuget.org/packages/MethodDecoratorEx.Fody/
	
### Planned

- [x] Make Init method optional
- [x] Add "this" as parameter to Init method if method is not static
- [ ] Pass return value to "OnExit" if method returns any

Fill free to request for features you want to see in this plugin.