import "./App.css";
import { CryoptixLogo } from "./components/CryoptixLogo";

function App() {
  return (
    <>
      {/* <CryoptixLogo variant="logo" />
      <CryoptixLogo variant="icon" width={48} />
      <CryoptixLogo variant="wordmark" width={180} /> */}
      <CryoptixLogo variant="logo" theme="auto" />
      {/* <CryoptixLogo variant="logo" theme="light" />
      <CryoptixLogo variant="logo" theme="dark" /> */}
    </>
  );
}

export default App;
