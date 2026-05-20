import { Link } from 'react-router-dom';
import { useAuth } from '../store/AuthContext';

export default function Navbar() {
  const { user, logout } = useAuth();

  return (
    <nav className="navbar">
      <Link to="/" className="brand">ShopWise</Link>
      <div className="nav-links">
        <Link to="/products">Products</Link>
        {user ? (
          <>
            <Link to="/cart">Cart</Link>
            <Link to="/orders">Orders</Link>
            <span>{user.fullName}</span>
            <button onClick={logout}>Logout</button>
          </>
        ) : (
          <>
            <Link to="/login">Login</Link>
            <Link to="/register">Register</Link>
          </>
        )}
      </div>
    </nav>
  );
}
