import React from 'react';
import './NotFound.scss';

const NotFoundPage = () => {
    return (
        <div className="error-container">
            <div>
                <span>404</span>
                <div className="vertical-spacer"></div>
                <span>Page Not Found</span>
            </div>
        </div>
    );
};

export default NotFoundPage;