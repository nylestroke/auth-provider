import ImageLayout from "../ImageLayout/ImageLayout";
import LoginImage from "./../login.jpg";
import RegisterImage from "./../register.jpg";
import NotificationsNoneIcon from "@mui/icons-material/NotificationsNone";
import Logo from "../../assets/img/logo.png";
import "./FormLayout.scss";

export default function FormLayout({ children }: any) {
	const url = window.location.pathname;
	return (
		<div className="layout-container">
			<div className="layout">
				<ImageLayout>
					{!url.startsWith("/authorize/register") ? (
						<img src={LoginImage} />
					) : (
						<img src={RegisterImage} />
					)}
				</ImageLayout>

				<div className="form-layout">
					<div className="minimal">
						<div className="logo">
							<img src={Logo} />
						</div>
						{children}
						<div className="blob-alert">
							<NotificationsNoneIcon />
							<div className="text">
								Since this site is in early access it may contain bugs!
							</div>
						</div>
					</div>
				</div>
			</div>
		</div>
	);
}
