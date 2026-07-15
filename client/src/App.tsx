import { AccountsList } from './components/AccountsList'
import { LoginForm } from './components/LoginForm'
import { RegisterForm } from './components/RegisterForm'
import { useAuth } from './context/AuthContext'
import './App.css'

function App() {
  const { token, account, logout } = useAuth();

  return (
    <div>
      <h1>Proof</h1>
      {token ? (
        <div>
          <p>Logged in as {account?.email}</p>
          <button onClick={logout}>Log Out</button>
          <AccountsList />
        </div>
      ) : (
        <div>
          <h2>Log In</h2>
          <LoginForm />
          <h2>Register</h2>
          <RegisterForm />
        </div>
      )}
    </div>
  )
}

export default App
