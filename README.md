<h1>An Observable Generic Tree Collection</h1>

<h2>Introduction</h2>

<p>In this tip, I will describe a basic <code>Tree</code> structure based on <code>ObservableCollection</code>, where each element may contain child elements of a same type. The number of nested levels is unlimited. This type of collection may become handy if you need to store or manipulate hierarchical data. Since collections are internally based on <code>ObservableCollection</code> class, the tree fits very well for WPF binding scenarios. For example, in the attached sample project, I used it to display data in the <code>TreeView</code> using <code>HierarchicalDataTemplate</code>. Any modification of original object graph gets reflected in the UI automatically, thanks to <strong>WPF Data Binding</strong>.</p>

<p>Another important aspect of this particular implementation of <code>Tree</code> is that my goal was to make the code as compact as possible and rely on the original implementation of <span style="color: rgb(153, 0, 0); font-family: Consolas, &quot;Courier New&quot;, Courier, mono; font-size: 14.66px;">ObservableCollection</span> as much as possible. As result, all manipulations with tree, like adding, removing, replacing nodes, etc. are supposed to be performed using well-known methods and properties of standard <code>IList&lt;T&gt;</code>.</p>

<h2>Interface</h2>

<p>The <code>Tree</code> class implements the following interfaces:</p>

<pre lang="cs">
public interface ITree
{
    bool HasItems { get; }
    void Add(object data);
}

public interface ITree&lt;T&gt; : ITree, IEnumerable&lt;ITree&lt;T&gt;&gt;
{
    T Data { get; }
    IList&lt;ITree&lt;T&gt;&gt; Items { get; }
    ITree&lt;T&gt; Parent { get; }
    IEnumerable&lt;ITree&lt;T&gt;&gt; GetParents(bool includingThis);
    IEnumerable&lt;ITree&lt;T&gt;&gt; GetChildren(bool includingThis);
}</pre>

<p><code>ITree</code> is non-generic base interface. The generic interface <code>ITree&lt;T&gt;</code> overrides <code>ITree</code> and exposes additional properties and methods that involve generic type. The reason of splitting the interface to non-generic and generic is to be able to easily identify instances compatible with <code>ITree</code> elements in the <code>Add</code> method.</p>

<p>Note that <code>ITree&lt;T&gt;</code> is also derived from <code>IEnumerable&lt;ITree&lt;T&gt;&gt;</code>. This is an indication that any element of the tree may have its own children. So you can easily iterate over a tree element without having to worry about presence or absence of children in it. It also provides syntactical sugar while using <code>Linq</code> or <code>foreach</code> constructs.</p>

<p>Below are methods and properties of <code>ITree&lt;T&gt;</code>:</p>

<ul>
	<li>?<code>HasItems</code> - returns <code>true</code> if the tree node contains any child elements.</li>
	<li><code>Add</code> - adds object(s) to children collection. Note that the method accepts object type as an argument. This is to support scenarios where you can add multiple objects at the same time. I will demonstrate this later.</li>
	<li><code>Data</code> - returns generic data object associated with current node.</li>
	<li><code>Items</code> - exposes a collection of child elements as <code>IList&lt;T&gt;</code>. You can use this property to add, remove, replace, reset children.</li>
	<li><code>GetParents()</code> - a method enumerating all parent nodes, if any. Optionally may include itself in the result.</li>
	<li><code>GetChildren()</code> - a method enumerating all child nodes, if any. Optionally may include itself in the result.</li>
</ul>

<h2>Implementation</h2>

<p>The <code>Tree</code> class implements the <code>ITree</code> and <code>ITree&lt;T&gt;</code> interfaces described above. In the constructor, you can pass a generic <code>Data</code> object as well as optional child nodes. When child nodes are passed, they are added using the following <code>Add</code> method:</p>

<pre lang="cs">
public void Add(object data)
{
    if (data == null)
        return;

    if (data is T)
    { 
        Items.Add(CloneNode(new Tree&lt;T&gt;((T) data)));
        return;
    }

    var t = data as ITree&lt;T&gt;;
    if (t != null)
    {
        Items.Add(CloneTree(t));
        return;
    }

    var o = data as object[];
    if (o != null)
    {
        foreach (var obj in o)
            Add(obj);
        return;
    }

    var e = data as IEnumerable;
    if (e != null &amp;&amp; !(data is ITree))
    {
        foreach (var obj in e)
            Add(obj);
        return;
    }

    throw new InvalidOperationException(&quot;Cannot add unknown content type.&quot;);
}</pre>

<p>As you can see, every child element gets cloned before adding to collection. This is necessary to avoid circular references that may lead to unpredictable behavior. As result, if you want to override the <code>Tree</code> class with your own version (for example, you may want to create a class that hides generic type like: <code>class PersonTree : Tree&lt;Person&gt;</code>), you would have to override virtual <code>CloneNode</code> method returning appropriate instance of your type.</p>

<p>Another aspect of <code>Add</code> method is that it can digest any type of <code>IEnumerable</code> or its derivatives as long as their elements are of type <code>ITree&lt;T&gt;</code> or simply <code>T</code>. Here is an example of <code>ITree&lt;string&gt;</code> constructed using various methods:</p>

<pre lang="cs">
var tree = Tree.Create(&quot;My Soccer Leagues&quot;,
    Tree.Create(&quot;League A&quot;,
        Tree.Create(&quot;Division A&quot;, 
            &quot;Team 1&quot;, 
            &quot;Team 2&quot;, 
            &quot;Team 3&quot;),
        Tree.Create(&quot;Division B&quot;, new List&lt;string&gt; {
            &quot;Team 4&quot;, 
            &quot;Team 5&quot;, 
            &quot;Team 6&quot;}),
        Tree.Create(&quot;Division C&quot;, new List&lt;ITree&lt;string&gt;&gt; {
            Tree.Create(&quot;Team 7&quot;), 
            Tree.Create(&quot;Team 8&quot;)})),
    Tree.Create(&quot;League B&quot;,
        Tree.Create(&quot;Division A&quot;, 
            new Tree&lt;string&gt;(&quot;Team 9&quot;), 
            new Tree&lt;string&gt;(&quot;Team 10&quot;), 
            new Tree&lt;string&gt;(&quot;Team 11&quot;)),
        Tree.Create(&quot;Division B&quot;, 
            Tree.Create(&quot;Team 12&quot;), 
            Tree.Create(&quot;Team 13&quot;), 
            Tree.Create(&quot;Team 14&quot;))));</pre>

<p>Finally, the implementation of <code>Parent</code> property. This property is essential to any hierarchical data structure because it allows you to walk up the tree providing access to the root nodes. By default, the <code>Items</code> collection of a node is not initialized (<code>m_items = null</code>). Once you try to access <code>Items</code> property or add an element to it, the instance of <code>ObservableCollection</code> is created as container for child nodes. The <span style="color: rgb(153, 0, 0); font-family: Consolas, &quot;Courier New&quot;, Courier, mono; font-size: 14.66px;">ObservableCollection</span>&#39;s <code>CollectionChanged?</code> event is internally hooked to a handler that assigns or unassigns the <code>Parent</code> property of the children being added or removed. Here is how the code looks:</p>

<pre lang="cs">
public IList&lt;ITree&lt;T&gt;&gt; Items
{
    get
    {
        if (m_items == null)
        {
            m_items = new ObservableCollection&lt;ITree&lt;T&gt;&gt;();
            m_items.CollectionChanged += ItemsOnCollectionChanged;
        }
        return m_items;
    }
}

private void ItemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
{
    if (args.Action == NotifyCollectionChangedAction.Add &amp;&amp; args.NewItems != null)
    {
        foreach (var item in args.NewItems.Cast&lt;Tree&lt;T&gt;&gt;())
        {
            item.Parent = this;
        }
    }
    else if (args.Action != NotifyCollectionChangedAction.Move &amp;&amp; args.OldItems != null)
    {
        foreach (var item in args.OldItems.Cast&lt;Tree&lt;T&gt;&gt;())
        {
            item.Parent = null;
            item.ResetOnCollectionChangedEvent();
        }
    }
}

private void ResetOnCollectionChangedEvent()
{
    if (m_items != null)
        m_items.CollectionChanged -= ItemsOnCollectionChanged;
}</pre>

<h2>Helper Tree Class</h2>

<p>The helper <code>static Tree</code> class provides syntactical sugar for creating tree nodes. For example, instead of writing:</p>

<pre lang="cs">
var tree = new Tree&lt;Person&gt;(new Person(&quot;Root&quot;),
    new Tree&lt;Person&gt;(new Person(&quot;Child #1&quot;)),
    new Tree&lt;Person&gt;(new Person(&quot;Child #2&quot;)),
    new Tree&lt;Person&gt;(new Person(&quot;Child #3&quot;)));</pre>

<p><span style="color: rgb(17, 17, 17); font-family: &quot;Segoe UI&quot;, Arial, sans-serif; font-size: 14px;">You could express the same code in a slightly cleaner way:</span></p>

<pre lang="cs">
var tree = Tree.Create(new Person(&quot;Root&quot;),
    Tree.Create(new Person(&quot;Child #1&quot;)),
    Tree.Create(new Person(&quot;Child #2&quot;)),
    Tree.Create(new Person(&quot;Child #3&quot;)));</pre>

<p>Also, there is a helper <code>Visit</code> method. It takes a tree node <code>ITree&lt;T&gt;</code> and an <code>Action&lt;ITree&lt;T&gt;&gt;</code> recursively traversing the tree node and calling the action for each node:</p>

<pre lang="cs">
public static void Visit&lt;T&gt;(ITree&lt;T&gt; tree, Action&lt;ITree&lt;T&gt;&gt; action)
{
    action(tree);
    if (tree.HasItems)
        foreach (var item in tree)
            Visit(item, action);
}</pre>

<p>For example, you can use this method to print a tree:</p>

<pre lang="cs">
public static string DumpElement(ITree&lt;string&gt; element)
{
    var sb = new StringBuilder();
    var offset = element.GetParents(false).Count();
    Tree.Visit(element, x =&gt;
    {
        sb.Append(new string(&#39; &#39;, x.GetParents(false).Count() - offset));
        sb.AppendLine(x.ToString());
    });
    return sb.ToString();
}</pre>

<h2>Summary</h2>

<p>The <code>Tree&lt;T&gt;</code> collection is a simple hierarchical data structure with the following features:</p>

<ul>
	<li>Supports generic data types</li>
	<li>Automatically maintains a reference to <code>Parent</code> node</li>
	<li>Friendly to WPF binding</li>
	<li>Fluid syntax of constructing and accessing elements</li>
	<li>Utilizes standard .NET <code>IList&lt;T&gt;</code> features and functionality</li>
</ul>
