import React, {useState} from 'react';
import { useNavigate } from 'react-router-dom';
import './../Login/Form.scss';
import VisibilityOffIcon from "@mui/icons-material/VisibilityOff";
import VisibilityIcon from "@mui/icons-material/Visibility";

const RegisterPage = () => {
  const [visibility, setVisibility] = useState<boolean>(false);
  const navigate = useNavigate();
  
  return (
      <div className="container">
        <div className="content">
          <div className="form">
            <div className="header">Sign up new account</div>
            <form>
              <div className="input_block">
                <input type="text" name="username" placeholder="Username"/>
              </div>
              <div className="input_block">
                <input type="text" name="email" placeholder="Your email"/>
              </div>
              <div className="input_block">
                <input type={visibility ? "text" : "password"} name="password" placeholder="Password"/>
                <button type="button" className="visibility" onMouseDown={() => setVisibility(true)}
                        onMouseUp={() => setVisibility(false)}
                >
                  {visibility ? (
                      <VisibilityOffIcon/>
                  ) : (
                      <VisibilityIcon/>
                  )}
                </button>
              </div>
              <div className="input_block">
                <input type={visibility ? "text" : "password"} name="cpassword" placeholder="Confirm password"/>
              </div>
              <div className="button_block">
                <button type="submit">Sign Up</button>
              </div>
              <div className="button_block secondary">
                <button type="button" onClick={() => navigate("/authorize")}>Back to login</button>
              </div>
              <div className="link_block">
                <div>Do you already have an account?
                  <span onClick={() => navigate("/authorize")}>Sign In</span>
                </div>
              </div>
            </form>
          </div>
          <div className="helper"></div>
        </div>
      </div>
  );
};

export default RegisterPage;
