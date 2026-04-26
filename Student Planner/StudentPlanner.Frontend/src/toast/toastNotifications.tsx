import { toast, Slide, type ToastOptions } from "react-toastify";

const baseToast: ToastOptions = {
  position: "top-right",
  autoClose: 2800,
  closeOnClick: true,
  pauseOnHover: true,
  draggable: false,
  hideProgressBar: true,
  transition: Slide,
};

export const successMessage = (message: string) => toast.success(message, {
    ...baseToast,
    className: "app-toast app-toast-success",
});


export const errorMessage = (message: string) => toast.error(message, {
    ...baseToast,
    className: "app-toast app-toast-error",
});

export const infoMessage = (message: string) => toast(
    <div className="system-toast">
      <span className="system-toast-icon">•</span>
      <span>{message}</span>
    </div>,
    {
      ...baseToast,
      className: "app-toast app-toast-info"
    }
);