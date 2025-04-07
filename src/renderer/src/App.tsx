import NavigationBar from "./components/NavigationBar";

function App(): JSX.Element {
    return (
        <div style={{width: "100vw", height: "100vh", backgroundColor: "var(--dark-gray)", display: "flex", padding: "4px", boxSizing: "border-box"}}>
            <NavigationBar/>
        </div>
    );
}

export default App
