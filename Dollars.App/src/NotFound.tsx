import { Link } from "react-router";

const NotFound = () => {
return (
    <div>
        <h1>404</h1>
        <Link to ={'/'}><button>Home</button></Link>
    </div>
)
}

export default NotFound;