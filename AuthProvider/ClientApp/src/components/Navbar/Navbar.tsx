import React from "react";
import "./Navbar.scss";
import NavLogo from "./../../assets/img/logo.png";

const NavbarComponent = () => {
	return (
		<nav>
			<div className="border"></div>
			<div className="logo">
				<img src={NavLogo} alt="logo" />
			</div>
			<div className="border"></div>
		</nav>
	);
};

export default NavbarComponent;
